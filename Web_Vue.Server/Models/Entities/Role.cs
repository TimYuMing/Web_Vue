using Web_Vue.Server.Models.Base;

namespace Web_Vue.Server.Models.Entities;

/// <summary> 角色資料表 </summary>
[Comment("角色資料表")]
public partial class Role : AuditEntity
{
    /// <summary> 角色名稱 </summary>
    [Required]
    [StringLength(50)]
    [Comment("角色名稱")]
    public required string Name { get; set; }

    /// <summary> 角色代碼 </summary>
    [Required]
    [StringLength(50)]
    [Comment("角色代碼")]
    public required string Code { get; set; }

    /// <summary> 備註 </summary>
    [StringLength(500)]
    [Comment("備註")]
    public required string Description { get; set; }

    /// <summary> 啟用狀態 </summary>
    [Comment("啟用狀態")]
    public bool IsEnable { get; set; }

    /// <summary> 是否系統維護中 </summary>
    [Comment("是否系統維護中")]
    public bool IsMaintaining { get; set; }

    /// <summary> 是否為系統管理者 </summary>
    [Comment("是否為系統管理者")]
    [DefaultValue(false)]
    public bool IsAdmin { get; set; }

}
