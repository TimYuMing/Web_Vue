using Web_Vue.Server.Models.Base;

namespace Web_Vue.Server.Models.Entities;

/// <summary> 使用者資訊資料表 </summary>
[Comment("使用者資訊資料表")]
public partial class UserProfile : AuditEntity
{
    /// <summary> 使用者ID </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public new int ID { get; set; }

    /// <summary> 姓名 </summary>
    [Required]
    [StringLength(100)]
    [Comment("姓名")]
    public required string Name { get; set; }

    /// <summary> 是否啟用 (1:啟用, 2:停用) </summary>
    [Comment("是否啟用 (1:啟用, 2:停用)")]
    public UserProfileStatus Status { get; set; }
}
