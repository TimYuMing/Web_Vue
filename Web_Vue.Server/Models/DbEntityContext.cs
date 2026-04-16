using System.Reflection;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Models.Interfaces;

namespace Web_Vue.Server.Models;

/// <summary> 應用程式資料庫 Context — DbSet 與 OnModelCreating </summary>
public partial class DbEntityContext : DbContext
{
    // ===================== DbSets =====================

    /// <summary> 使用者帳號資料表 </summary>
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    /// <summary> 使用者資訊資料表 </summary>
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    /// <summary> 角色資料表 </summary>
    public DbSet<Role> Roles => Set<Role>();

    /// <summary> 角色使用者關聯表 </summary>
    public DbSet<RoleInUserAccount> RoleInUserAccounts => Set<RoleInUserAccount>();

    // ===================== OnModelCreating =====================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RoleInUserAccount 複合主鍵
        modelBuilder.Entity<RoleInUserAccount>()
            .HasKey(r => new { r.UserAccountID, r.RoleID });

        // 對所有實作 IUpdateAudit 的實體套用軟刪除全域查詢篩選
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IUpdateAudit).IsAssignableFrom(entityType.ClrType))
            {
                var method = SetSoftDeleteFilterMethod.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, [modelBuilder]);
            }
        }
    }

    private static readonly MethodInfo SetSoftDeleteFilterMethod =
        typeof(DbEntityContext).GetMethod(nameof(SetSoftDeleteFilter),
            BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void SetSoftDeleteFilter<T>(ModelBuilder modelBuilder)
        where T : class, IUpdateAudit
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => !e.IsDelete);
    }
}
