using Web_Vue.Server.Models;

namespace Web_Vue.Server.Controllers;

/// <summary> 測試用 API 控制器 </summary>
public class TestController : BaseController
{
    /// <summary>
    /// 測試 POST，示範 FluentValidation + 語系 + 統一回應
    /// </summary>
    /// <param name="request">請求模型</param>
    /// <param name="validator">注入的驗證器</param>
    [HttpPost]
    public async Task<ActionResult<ResponseModel>> Post(
        [FromBody] TestRequestModel request,
        [FromServices] IValidator<TestRequestModel> validator)
    {
        var result = await validator.ValidateAsync(request);

        if (!result.IsValid)
            return Ok(new ResponseModel
            {
                Status = ResultType.Fail,
                Message = Resx["Txt_操作失敗"],
                ErrorList = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .Select(g => new ResponseErrorDataViewModel
                    {
                        Key = g.Key,
                        ErrorTextList = g.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList()
            });

        return Ok(new ResponseModel
        {
            Status = ResultType.Success,
            Message = Resx["Txt_操作成功"],
            Data = new { Echo = request.Name, Timestamp = DateTime.UtcNow }
        });
    }
}
