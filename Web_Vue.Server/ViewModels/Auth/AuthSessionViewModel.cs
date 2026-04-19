namespace Web_Vue.Server.ViewModels.Auth;

/// <summary> 登入 session 資料（登入成功 / me / refresh 共用） </summary>
public class AuthSessionViewModel
{
    /// <summary> 使用者基本資訊 </summary>
    public AuthUserViewModel User { get; set; } = new();

    /// <summary> 使用者擁有的權限代碼清單 </summary>
    public List<string> Permissions { get; set; } = [];

    /// <summary> Session 過期時間 (UTC) </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary> 使用者基本資訊 </summary>
public class AuthUserViewModel
{
    /// <summary> 使用者 ID </summary>
    public int Id { get; set; }

    /// <summary> 帳號 </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary> 姓名 </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary> 是否為管理者 </summary>
    public bool IsAdmin { get; set; }

    /// <summary> 系統是否維護中 </summary>
    public bool IsMaintaining { get; set; }
}
