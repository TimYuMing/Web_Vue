using Web_Vue.Server.Models;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Repositories.Base;

namespace Web_Vue.Server.Repositories;

/// <summary> 角色權限關聯 Repository (A = RoleID, B = PermissionID) </summary>
public class PermissionInRoleRepository : BaseMultipleRepository<PermissionInRole>
{
    /// <summary> 建構子 </summary>
    public PermissionInRoleRepository(DbEntityContext context) : base(context) { }

    /// <inheritdoc/>
    public override IQueryable<PermissionInRole> FindAllByAId(int aid)
        => FindAll().Where(x => x.RoleID == aid);

    /// <inheritdoc/>
    public override async Task<List<int>> FindAllBIdByAIdAsync(int aid)
        => await FindAllByAId(aid).Select(x => x.PermissionID).ToListAsync();

    /// <inheritdoc/>
    public override async Task ClearByAIdAsync(int aid)
    {
        var entities = FindAllByAId(aid);
        Context.RemoveRange(entities);
        await Context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public override IQueryable<PermissionInRole> FindAllByBId(int bid)
        => FindAll().Where(x => x.PermissionID == bid);

    /// <inheritdoc/>
    public override async Task<List<int>> FindAllAIdByBIdAsync(int bid)
        => await FindAllByBId(bid).Select(x => x.RoleID).ToListAsync();

    /// <inheritdoc/>
    public override async Task ClearByBIdAsync(int bid)
    {
        var entities = FindAllByBId(bid);
        Context.RemoveRange(entities);
        await Context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public override async Task InsertRangeAsync(IEnumerable<PermissionInRole> dataList)
    {
        await Context.AddRangeAsync(dataList);
        await Context.SaveChangesAsync();
    }
}
