using System.Text.Json;
using Web_Vue.Server.ViewModels.Auth;

namespace Web_Vue.Server.Tools;

/// <summary> 使用者資訊工具 — 負責將 UserInfoModel 寫入或從 JWT Claims 讀出 </summary>
public static class UserInfoTool
{
    /// <summary> JWT Claim 中存放 UserInfoModel 所使用的 Key </summary>
    public const string UserKey = "__UserInfoKey";

    /// <summary> 從 ClaimsPrincipal 反序列化取得使用者資訊，未登入或解析失敗時回傳 null </summary>
    public static UserInfoViewModel? GetUserInfo(ClaimsPrincipal? user)
    {
        try
        {
            if (user == null) return null;

            var json = user.Claims.FirstOrDefault(x => x.Type == UserKey)?.Value;
            if (string.IsNullOrEmpty(json)) return null;

            return JsonSerializer.Deserialize<UserInfoViewModel>(json);
        }
        catch
        {
            return null;
        }
    }
}
