namespace Web_Vue.Server.Models;

/// <summary>
/// 統一 API 回應模型
/// </summary>
public class ResponseModel
{
    /// <summary>
    /// 回應結果狀態
    /// </summary>
    public ResultType Status { get; set; }

    /// <summary>
    /// 回應資料內容
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 回應訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 錯誤清單
    /// </summary>
    public List<ResponseErrorDataViewModel> ErrorList { get; set; } = new();
}

/// <summary>
/// 回應錯誤資料模型
/// </summary>
public class ResponseErrorDataViewModel
{
    /// <summary> Name </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary> Error Text List </summary>
    public List<string> ErrorTextList { get; set; } = new();
}
