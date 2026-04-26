using Web_Vue.Server.Models;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Repositories.Base;

namespace Web_Vue.Server.Repositories;

/// <summary> 角色權限關聯 Repository（PermissionInRole）</summary>
public class PermissionInRoleRepository : BaseMultipleRepository<PermissionInRole>
{
    /// <summary> 建構子 </summary>
    public PermissionInRoleRepository(DbEntityContext context) : base(context) { }

    /// <summary> 查詢指定角色的所有權限關聯 </summary>
    public override IQueryable<PermissionInRole> FindAllByAId(int aid)
        => FindAll().Where(x => x.RoleID == aid);

    /// <summary> 查詢指定角色擁有的所有 PermissionID 清單 </summary>
    public override async Task<List<int>> FindAllBIdByAIdAsync(int aid)
        => await FindAllByAId(aid).Select(x => x.PermissionID).ToListAsync();

    /// <summary> 刪除指定角色的所有權限關聯 </summary>
    public override async Task ClearByAIdAsync(int aid)
    {
        var entities = FindAllByAId(aid);
        Context.RemoveRange(entities);
        await Context.SaveChangesAsync();
    }

    /// <summary> 查詢擁有指定權限的所有角色關聯 </summary>
    public override IQueryable<PermissionInRole> FindAllByBId(int bid)
        => FindAll().Where(x => x.PermissionID == bid);

    /// <summary> 查詢擁有指定權限的所有 RoleID 清單 </summary>
    public override async Task<List<int>> FindAllAIdByBIdAsync(int bid)
        => await FindAllByBId(bid).Select(x => x.RoleID).ToListAsync();

    /// <summary> 刪除所有擁有指定權限的角色關聯 </summary>
    public override async Task ClearByBIdAsync(int bid)
    {
        var entities = FindAllByBId(bid);
        Context.RemoveRange(entities);
        await Context.SaveChangesAsync();
    }

    /// <summary> 批次新增角色權限關聯資料 </summary>
    public override async Task InsertRangeAsync(IEnumerable<PermissionInRole> dataList)
    {
        await Context.AddRangeAsync(dataList);
        await Context.SaveChangesAsync();
    }
}
