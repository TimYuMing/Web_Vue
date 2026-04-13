namespace Web_Vue.Server.Entities;

/// <summary> 角色使用者關聯表(多對多) </summary>
[Comment("角色使用者關聯表(多對多)")]
public partial class RoleInUserAccount
{
    /// <summary> 使用者ID </summary>
    [Key]
    [Comment("使用者ID")]
    public int UserAccountID { get; set; }

    /// <summary> 角色ID </summary>
    [Key]
    [Comment("角色ID")]
    public int RoleID { get; set; }
}
