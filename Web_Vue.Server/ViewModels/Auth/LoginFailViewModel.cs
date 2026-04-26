namespace Web_Vue.Server.ViewModels.Auth;

/// <summary> 登入失敗的附加資訊（放在 ResponseViewModel.Data） </summary>
public class LoginFailViewModel
{
    /// <summary> 失敗類型 </summary>
    public LoginFailType FailType { get; set; }

    /// <summary> 建議導向路徑（例如密碼過期時導向 /forget-password） </summary>
    public string? RedirectPath { get; set; }
}
