using System.Text.Json;
using Web_Vue.Server.ViewModels.Auth;

namespace Web_Vue.Server.Tools;

/// <summary> 使用者資訊工具 — 負責將 UserInfoModel 寫入或從 JWT Claims 讀出 </summary>
public static class UserInfoTool
{
    // ── Claim Type 常數 ──

    /// <summary> 使用者 ID Claim Type </summary>
    public const string UserIdClaimType = "uid";

    /// <summary> 帳號 Claim Type </summary>
    public const string AccountClaimType = "account";

    /// <summary> 使用者名稱 Claim Type </summary>
    public const string NameClaimType = "name";

    /// <summary> 角色清單 Claim Type（逗號分隔的 Role ID） </summary>
    public const string RoleListClaimType = "roles";

    /// <summary> 是否為管理者 Claim Type </summary>
    public const string IsAdminClaimType = "isAdmin";

    /// <summary> 是否維護中 Claim Type </summary>
    public const string IsMaintainingClaimType = "isMaintaining";

    /// <summary> 舊版 JSON 序列化 Claim Key（向下相容用） </summary>
    internal const string LegacyUserKey = "__UserInfoKey";

    // ── 解析 ──

    /// <summary>
    /// 從 ClaimsPrincipal 反序列化取得使用者資訊，未登入或解析失敗時回傳 null。
    /// 優先讀取個別 Claim，若不存在則回退至舊版 JSON Claim 格式。
    /// </summary>
    public static UserInfoViewModel? GetUserInfo(ClaimsPrincipal? user)
    {
        try
        {
            if (user == null)
            {
                return null;
            }

            // 新版：個別 Claim
            var idText = user.FindFirstValue(UserIdClaimType)
                      ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(idText, out var userId) && userId > 0)
            {
                var roleText = user.FindFirstValue(RoleListClaimType) ?? string.Empty;
                var roleList = roleText
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(r => int.TryParse(r, out var rid) ? rid : 0)
                    .Where(rid => rid > 0)
                    .Distinct()
                    .ToList();

                _ = bool.TryParse(user.FindFirstValue(IsAdminClaimType), out var isAdmin);
                _ = bool.TryParse(user.FindFirstValue(IsMaintainingClaimType), out var isMaintaining);

                return new UserInfoViewModel
                {
                    ID = userId,
                    Account = user.FindFirstValue(AccountClaimType) ?? string.Empty,
                    Name = user.FindFirstValue(NameClaimType)
                                ?? user.FindFirstValue(ClaimTypes.Name)
                                ?? string.Empty,
                    RoleList = roleList,
                    IsAdmin = isAdmin,
                    IsMaintaining = isMaintaining,
                };
            }

            // 舊版回退：完整 JSON
            var json = user.Claims.FirstOrDefault(x => x.Type == LegacyUserKey)?.Value;
            if (!string.IsNullOrEmpty(json))
            {
                return JsonSerializer.Deserialize<UserInfoViewModel>(json);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
