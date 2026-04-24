namespace Web_Vue.Server.Tools;

/// <summary>
/// 驗證錯誤清單建構工具
/// — 支援同時傳入 FluentValidation 結果與 ModelState，合併輸出統一 ErrorList
/// </summary>
public static class ValidationTool
{
    /// <summary>
    /// 將 FluentValidation 結果 和/或 ModelState 合併為統一 <see cref="ResponseErrorDataViewModel"/> 清單。
    /// 兩個參數皆可選，傳入哪個就合併哪個。
    /// </summary>
    public static List<ResponseErrorDataViewModel> BuildErrorList(
        FluentValidation.Results.ValidationResult? validationResult = null,
        ModelStateDictionary? modelState = null)
    {
        var result = new List<ResponseErrorDataViewModel>();

        if (validationResult is { Errors.Count: > 0 })
        {
            result.AddRange(
                validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .Select(g => new ResponseErrorDataViewModel
                    {
                        Key = g.Key,
                        ErrorTextList = g.Select(e => e.ErrorMessage).ToList(),
                    }));
        }

        if (modelState != null)
        {
            result.AddRange(
                modelState
                    .Where(kv => kv.Value?.Errors.Count > 0)
                    .Select(kv => new ResponseErrorDataViewModel
                    {
                        Key = kv.Key,
                        ErrorTextList = kv.Value!.Errors.Select(e => e.ErrorMessage).ToList(),
                    }));
        }

        return result;
    }
}
