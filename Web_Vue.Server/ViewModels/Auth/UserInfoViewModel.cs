namespace Web_Vue.Server.ViewModels.Auth;

/// <summary> 使用者資料模型（序列化後存入 JWT Claims） </summary>
public class UserInfoViewModel
{
    /// <summary> 使用者 ID </summary>
    public int ID { get; set; }

    /// <summary> 帳號 </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary> 姓名 </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary> 帳號狀態 </summary>
    public UserProfileStatus Status { get; set; }

    /// <summary> 最後登入日期 </summary>
    public DateTime? LatestLoginDate { get; set; }

    /// <summary> 為暫時密碼 </summary>
    public bool IsTemplatePassword { get; set; }

    /// <summary> 暫時密碼驗證有效期 </summary>
    public DateTime? ValidPasswordExpireTime { get; set; }

    /// <summary> 是否已登入（ID > 0 即表示已登入） </summary>
    public bool IsLogin => ID > 0;

    /// <summary> 所屬角色 ID 清單 </summary>
    public List<int> RoleList { get; set; } = new();

    /// <summary> 是否為管理者 </summary>
    public bool IsAdmin { get; set; }

    /// <summary> 角色是否系統維護中 </summary>
    public bool IsMaintaining { get; set; }
}
