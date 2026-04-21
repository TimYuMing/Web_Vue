using System.Reflection;
using Web_Vue.Server.Models.Entities;
using Web_Vue.Server.Interfaces;

namespace Web_Vue.Server.Models;

/// <summary> 應用程式資料庫 Context — DbSet 與 OnModelCreating </summary>
public partial class DbEntityContext : DbContext
{
    // ===================== DbSets =====================

    /// <summary> 使用者帳號資料表 </summary>
    public DbSet<UserAccount> UserAccount => Set<UserAccount>();

    /// <summary> 使用者資訊資料表 </summary>
    public DbSet<UserProfile> UserProfile => Set<UserProfile>();

    /// <summary> 角色資料表 </summary>
    public DbSet<Role> Role => Set<Role>();

    /// <summary> 角色使用者關聯表 </summary>
    public DbSet<RoleInUserAccount> RoleInUserAccount => Set<RoleInUserAccount>();

    /// <summary> 權限資料表 </summary>
    public DbSet<Permission> Permission => Set<Permission>();

    /// <summary> 權限角色關聯表 </summary>
    public DbSet<PermissionInRole> PermissionInRole => Set<PermissionInRole>();

    // ===================== OnModelCreating =====================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // RoleInUserAccount 複合主鍵
        modelBuilder.Entity<RoleInUserAccount>()
            .HasKey(r => new { r.UserAccountID, r.RoleID });

        // PermissionInRole 複合主鍵
        modelBuilder.Entity<PermissionInRole>()
            .HasKey(p => new { p.RoleID, p.PermissionID });

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
