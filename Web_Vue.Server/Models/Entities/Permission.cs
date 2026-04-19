using Web_Vue.Server.Models.Base;

namespace Web_Vue.Server.Models.Entities;

/// <summary> 權限資料表 </summary>
[Comment("權限資料表")]
public partial class Permission : AuditEntity
{
    /// <summary> 權限代碼 </summary>
    [Required]
    [StringLength(500)]
    [Comment("權限代碼")]
    public required string Code { get; set; }

    /// <summary> 父節點 </summary>
    [Comment("父節點")]
    public int? ParentID { get; set; }

    /// <summary> 排序 </summary>
    [Comment("排序")]
    public int Sort { get; set; }

    /// <summary> 權限名稱 </summary>
    [Required]
    [StringLength(500)]
    [Comment("權限名稱")]
    public required string Name { get; set; }

    /// <summary> 路徑 </summary>
    [Required]
    [StringLength(500)]
    [Comment("路徑")]
    public required string Url { get; set; }

    /// <summary> 是否為資料夾節點（true = 資料夾，false = 功能葉節點） </summary>
    [Comment("是否為資料夾節點")]
    public bool IsFolder { get; set; }

    /// <summary> 是否顯示於權限清單上 </summary>
    [Comment("是否顯示於權限清單上")]
    public bool IsDisplay { get; set; }

    /// <summary> 是否啟用 </summary>
    [Comment("是否啟用")]
    public bool IsEnable { get; set; }
}
