using System.Reflection;
using Web_Vue.Server._Attribute;
using Web_Vue.Server.Interfaces;
using Web_Vue.Server.Models;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Repositories;

namespace Web_Vue.Server.Tools;

/// <summary>
/// 權限控制工具
/// — 提供權限掃描（自動同步 DB 與 PermissionCodes 定義）及使用者權限查詢功能
/// </summary>
public class PermissionTool(
    DbEntityContext context,
    IBaseRepository<Permission> permissionRe,
    IBaseRepository<Role> roleRe)
{
    private readonly IBaseMultipleRepository<PermissionInRole> _permissionInRoleRe
        = new PermissionInRoleRepository(context);

    private List<PermissionCacheItem>? _permissionCacheList;

    // ===================== 掃描 =====================

    /// <summary>
    /// 掃描 <see cref="Web_Vue.Server._Authorization.PermissionCodes"/> 下所有巢狀 class 與 const 欄位，
    /// 自動在資料庫中新增、更新或刪除對應的 <see cref="Permission"/> 及 <see cref="PermissionInRole"/> 資料。
    /// </summary>
    public async Task ScanningAsync()
    {
        var existPermissionList = await permissionRe.FindAll().ToListAsync();

        // 資料夾節點：key = 路徑碼（如 "Backend.Linkchain"），value = DB 的 Permission.ID
        var parentCodeDic = new Dictionary<string, int>();
        // 葉節點代碼清單（GUID 字串）
        var childCodeList = new List<string>();

        // 取得所有管理者角色及其已擁有的權限 ID
        var adminRoles = await roleRe.FindAll().Where(r => r.IsAdmin).ToListAsync();
        var adminRolePermissionMap = new Dictionary<int, List<int>>();
        foreach (var role in adminRoles)
            adminRolePermissionMap[role.ID] = await _permissionInRoleRe.FindAllBIdByAIdAsync(role.ID);

        // 掃描 PermissionCodes 下所有巢狀 Class（任意深度），按 FullName 排序確保父層先處理
        var authClasses = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.FullName != null &&
                        t.FullName.StartsWith("Web_Vue.Server._Authorization.PermissionCodes+"))
            .OrderBy(t => t.FullName);

        await using var transaction = await context.Database.BeginTransactionAsync();

        foreach (var ac in authClasses)
        {
            var parentAttr = ac.GetCustomAttributes<PermissionAttribute>()
                               .FirstOrDefault()
                           ?? new PermissionAttribute(string.Empty, 0, isFolder: true);

            var folderCode = GetFolderCode(ac);
            var parentFolderCode = GetParentFolderCode(ac);
            var parentDbId = parentFolderCode != null && parentCodeDic.TryGetValue(parentFolderCode, out var pid)
                ? (int?)pid : null;

            // ── 父節點（資料夾）處理 ──
            var parentPermission = existPermissionList.FirstOrDefault(p => p.Code == folderCode);

            if (parentPermission == null)
            {
                parentPermission = new Permission
                {
                    Code = folderCode,
                    ParentID = parentDbId,
                    Name = parentAttr.Name,
                    Sort = parentAttr.Sort,
                    IsFolder = parentAttr.IsFolder,
                    Url = parentAttr.Url,
                    IsDisplay = parentAttr.IsDisplay,
                    IsEnable = true
                };
                await permissionRe.InsertAsync(parentPermission);
            }
            else
            {
                if (UpdatePermissionAttrs(parentPermission, parentAttr, parentDbId))
                    await permissionRe.UpdateAsync(parentPermission);
            }

            await AssignToAdminRolesAsync(parentPermission.ID, adminRolePermissionMap);
            parentCodeDic[folderCode] = parentPermission.ID;

            // ── 子節點（葉節點 const 欄位）處理 ──
            foreach (var field in ac.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var childAttr = field.GetCustomAttributes<PermissionAttribute>()
                                     .FirstOrDefault()
                                 ?? new PermissionAttribute(string.Empty, 0);

                var childCode = field.GetValue(null)?.ToString();
                if (string.IsNullOrEmpty(childCode)) continue;

                var childPermission = existPermissionList.FirstOrDefault(p => p.Code == childCode);

                if (childPermission == null)
                {
                    childPermission = new Permission
                    {
                        ParentID = parentPermission.ID,
                        Code = childCode,
                        Sort = childAttr.Sort,
                        IsFolder = childAttr.IsFolder,
                        Name = childAttr.Name,
                        Url = childAttr.Url,
                        IsDisplay = childAttr.IsDisplay,
                        IsEnable = true
                    };
                    await permissionRe.InsertAsync(childPermission);
                }
                else
                {
                    var needsUpdate = childPermission.ParentID != parentPermission.ID;
                    if (needsUpdate) childPermission.ParentID = parentPermission.ID;

                    needsUpdate |= UpdatePermissionAttrs(childPermission, childAttr, null);
                    if (needsUpdate) await permissionRe.UpdateAsync(childPermission);
                }

                await AssignToAdminRolesAsync(childPermission.ID, adminRolePermissionMap);
                childCodeList.Add(childPermission.Code);
            }
        }

        // ── 刪除 DB 中已不存在於 PermissionCodes 的節點 ──
        var toDelete = existPermissionList
            .Where(p => !parentCodeDic.ContainsKey(p.Code) && !childCodeList.Contains(p.Code))
            .ToList();

        foreach (var dp in toDelete)
        {
            // 先移除關聯的角色權限紀錄
            var pirList = await _permissionInRoleRe.FindAllByBId(dp.ID).ToListAsync();
            if (pirList.Count > 0)
                context.PermissionInRoles.RemoveRange(pirList);

            // 軟刪除權限（實體已被追蹤，直接標記）
            dp.IsDelete = true;
        }

        if (toDelete.Count > 0)
            await context.SaveChangesAsync();

        await transaction.CommitAsync();

        // 重置快取
        _permissionCacheList = null;
    }

    // ===================== 權限查詢 =====================

    /// <summary> 取得使用者所有擁有的權限 ID 清單 </summary>
    public async Task<List<int>> GetUserPermissionIdsAsync(List<int> roleIds)
    {
        return await (
            from pir in _permissionInRoleRe.FindAll()
            where roleIds.Contains(pir.RoleID)
            select pir.PermissionID
        ).Distinct().ToListAsync();
    }

    /// <summary> 取得後台麵包屑階層清單 </summary>
    public async Task<List<BreadCrumbItem>> GetBreadCrumbListAsync(string permissionCode)
    {
        var list = new List<BreadCrumbItem>();
        var cache = await GetPermissionCacheListAsync();

        var current = cache.Find(x => x.Code == permissionCode);
        while (current != null)
        {
            list.Insert(0, new BreadCrumbItem
            {
                Code = current.Code,
                Name = current.Name,
                Url = current.Url,
                IsLastNode = current.Code == permissionCode
            });
            current = cache.Find(x => x.Id == current.ParentId);
        }

        return list;
    }

    // ===================== 私有工具方法 =====================

    /// <summary> 將指定權限指派給所有管理者角色（若尚未擁有） </summary>
    private async Task AssignToAdminRolesAsync(int permissionId, Dictionary<int, List<int>> adminRolePermissionMap)
    {
        foreach (var (roleId, permIds) in adminRolePermissionMap)
        {
            if (permIds.Contains(permissionId)) continue;
            await _permissionInRoleRe.InsertRangeAsync(
            [
                new PermissionInRole { RoleID = roleId, PermissionID = permissionId }
            ]);
            permIds.Add(permissionId);
        }
    }

    /// <summary>
    /// 比對並更新 Permission 屬性，有異動時回傳 true。
    /// <paramref name="parentDbId"/> 傳入時一併比對父節點。
    /// </summary>
    private static bool UpdatePermissionAttrs(
        Permission permission, PermissionAttribute attr, int? parentDbId)
    {
        var isUpdate = false;

        if (parentDbId.HasValue && permission.ParentID != parentDbId)
        {
            permission.ParentID = parentDbId;
            isUpdate = true;
        }

        if (permission.Name != attr.Name)
        {
            permission.Name = attr.Name;
            isUpdate = true;
        }

        if (permission.Sort != attr.Sort)
        {
            permission.Sort = attr.Sort;
            isUpdate = true;
        }

        if (permission.IsFolder != attr.IsFolder)
        {
            permission.IsFolder = attr.IsFolder;
            isUpdate = true;
        }

        if (permission.Url != attr.Url)
        {
            permission.Url = attr.Url;
            isUpdate = true;
        }

        if (permission.IsDisplay != attr.IsDisplay)
        {
            permission.IsDisplay = attr.IsDisplay;
            isUpdate = true;
        }

        return isUpdate;
    }

    // ── 型別階層工具 ──

    /// <summary>
    /// 取得資料夾節點的唯一路徑碼。
    /// 例：PermissionCodes+Backend+Linkchain → "Backend.Linkchain"
    /// </summary>
    private static string GetFolderCode(Type type)
    {
        var parts = new List<string>();
        var current = type;
        while (current != null && current.Name != "PermissionCodes")
        {
            parts.Insert(0, current.Name);
            current = current.DeclaringType;
        }
        return string.Join(".", parts);
    }

    /// <summary>
    /// 取得父資料夾節點的路徑碼；若父層為 PermissionCodes 本身（即根層）則回傳 null。
    /// </summary>
    private static string? GetParentFolderCode(Type type)
    {
        var declaring = type.DeclaringType;
        if (declaring == null || declaring.Name == "PermissionCodes") return null;
        return GetFolderCode(declaring);
    }

    // ── 快取 ──

    private async Task<List<PermissionCacheItem>> GetPermissionCacheListAsync()
    {
        if (_permissionCacheList != null) return _permissionCacheList;

        _permissionCacheList = await permissionRe.FindAll()
            .Select(p => new PermissionCacheItem
            {
                Id = p.ID,
                ParentId = p.ParentID,
                Code = p.Code,
                Name = p.Name,
                Url = p.Url
            })
            .AsNoTracking()
            .ToListAsync();

        return _permissionCacheList;
    }

    // ── 內部模型 ──

    private sealed class PermissionCacheItem
    {
        public int Id { get; init; }
        public int? ParentId { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
    }

    /// <summary> 麵包屑項目 </summary>
    public sealed class BreadCrumbItem
    {
        /// <summary> 權限代碼 </summary>
        public string Code { get; init; } = string.Empty;

        /// <summary> 名稱 </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary> 路徑 </summary>
        public string Url { get; init; } = string.Empty;

        /// <summary> 是否為最末節點 </summary>
        public bool IsLastNode { get; init; }
    }
}
