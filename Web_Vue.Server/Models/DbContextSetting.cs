using Web_Vue.Server.Models.Interfaces;
using Web_Vue.Server.Services;

namespace Web_Vue.Server.Models;

/// <summary> 應用程式資料庫 Context — 建構子、SaveChanges、稽核欄位邏輯 </summary>
public partial class DbEntityContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public DbEntityContext(DbContextOptions<DbEntityContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // ===================== SaveChanges =====================

    /// <summary>
    /// 儲存變更前自動填入稽核欄位：
    /// <list type="bullet">
    ///   <item>新增時：填入 CreateUser / CreateTime (若實作 <see cref="ICreateAudit"/>)</item>
    ///   <item>新增 / 修改時：填入 UpdateUser / UpdateTime (若實作 <see cref="IUpdateAudit"/>)</item>
    ///   <item>自動為空的 Guid 主鍵產生 NewGuid (若實作 <see cref="IStringEntity"/>)</item>
    /// </list>
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }

    private void ApplyAuditFields()
    {
        // 使用 UTC+8 台灣時間
        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(8)).DateTime;
        var account = _currentUserService.UserInfo?.Account;

        foreach (var entry in ChangeTracker.Entries())
        {
            // 自動為空的 Guid 主鍵產生值
            if (entry.State == EntityState.Added && entry.Entity is IStringEntity stringEntity && stringEntity.ID == Guid.Empty)
            {
                stringEntity.ID = Guid.NewGuid();
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    // 填入建立稽核
                    if (entry.Entity is ICreateAudit createAudit)
                    {
                        createAudit.CreateUser = account;
                        createAudit.CreateTime = now;
                    }
                    // 填入更新稽核 (新增時也初始化)
                    if (entry.Entity is IUpdateAudit addUpdateAudit)
                    {
                        addUpdateAudit.UpdateUser = account;
                        addUpdateAudit.UpdateTime = now;
                    }
                    break;

                case EntityState.Modified:
                    // 填入更新稽核
                    if (entry.Entity is IUpdateAudit updateAudit)
                    {
                        updateAudit.UpdateUser = account;
                        updateAudit.UpdateTime = now;
                    }
                    // 保護建立稽核欄位不被覆寫
                    if (entry.Entity is ICreateAudit)
                    {
                        entry.Property(nameof(ICreateAudit.CreateUser)).IsModified = false;
                        entry.Property(nameof(ICreateAudit.CreateTime)).IsModified = false;
                    }
                    break;
            }
        }
    }
}
