namespace Web_Vue.Server.Enums;

/// <summary>
/// 回應結果類型
/// </summary>
public enum ResultType
{
    /// <summary> 成功 </summary>
    [Description("Txt_成功")]
    Success = 1,

    /// <summary> 失敗 </summary>
    [Description("Txt_失敗")]
    Fail = 2,

    /// <summary> 確認 </summary>
    [Description("Txt_確認")]
    Confirm = 3
}

/// <summary> 帳號狀態 列舉 </summary>
public enum UserProfileStatus
{
    /// <summary> 啟用 </summary>
    [Description("Txt_啟用")]
    Enable = 1,

    /// <summary> 停用 </summary>
    [Description("Txt_停用")]
    Disable = 2,
}