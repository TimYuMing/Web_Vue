using Web_Vue.Server.Models.Base;

namespace Web_Vue.Server.Models.Entities;

/// <summary> 使用者帳號 </summary>
[Comment("使用者帳號")]
public class UserAccount : EntityBase
{
    /// <summary> 帳號 </summary>
    [Required]
    [StringLength(100)]
    [Unicode(false)]
    [Comment("帳號")]
    public required string Account { get; set; }

    /// <summary> 密碼 </summary>
    [Required]
    [StringLength(255)]
    [Unicode(false)]
    [Comment("密碼")]
    public required string Password { get; set; }

    /// <summary> 最後登入日期 </summary>
    [Comment("最後登入日期")]
    public DateTime? LatestLoginDate { get; set; }

    /// <summary> 驗證密碼有效期限 </summary>
    [Comment("驗證密碼有效期限")]
    public DateTime? ValidPasswordExpireTime { get; set; }

    /// <summary> 為暫時密碼 </summary>
    [Comment("為暫時密碼")]
    [DefaultValue(false)]
    public bool IsTemplatePassword { get; set; }
}
