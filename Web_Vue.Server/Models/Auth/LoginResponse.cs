namespace Web_Vue.Server.Models.Auth;

/// <summary> 登入成功回應 DTO </summary>
public class LoginResponse
{
    /// <summary> JWT Token </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary> 過期時間 (分鐘) </summary>
    public int ExpireMinutes { get; set; }

    /// <summary> 使用者帳號 </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary> 姓名 </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary> 是否為管理者 </summary>
    public bool IsAdmin { get; set; }

    /// <summary> 所屬角色 ID 清單 </summary>
    public List<int> RoleList { get; set; } = new();
}
