namespace Web_Vue.Server.Models.Entities;

/// <summary> 權限角色關聯表(多對多) </summary>
[Comment("權限角色關聯表(多對多)")]
public partial class PermissionInRole
{
    /// <summary> 角色ID </summary>
    [Key]
    [Comment("角色ID")]
    public int RoleID { get; set; }

    /// <summary> 權限ID </summary>
    [Key]
    [Comment("權限ID")]
    public int PermissionID { get; set; }
}
