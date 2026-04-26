namespace Web_Vue.Server.Controllers;

/// <summary>
/// API 控制器基底類別，所有控制器癃6應繼承此類別
/// 可在此類別上掛載共用的 Filter、授權、Exception 處理等
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private IStringLocalizer<SharedResource>? _resx;

    /// <summary> 語系檔 </summary>
    protected IStringLocalizer<SharedResource> Resx => _resx ??= HttpContext.RequestServices.GetRequiredService<IStringLocalizer<SharedResource>>();


    /// <summary> 200 OK — Success </summary>
    protected OkObjectResult Success(string msg = "", object? data = null, List<ResponseErrorDataViewModel>? errorList = null)
        => Ok(new ResponseViewModel<object> { Status = ResultType.Success, Message = msg, Data = data, ErrorList = errorList ?? [] });

    /// <summary> 200 OK — Fail </summary>
    protected OkObjectResult Fail(string msg = "", object? data = null, List<ResponseErrorDataViewModel>? errorList = null)
        => Ok(new ResponseViewModel<object> { Status = ResultType.Fail, Message = msg, Data = data, ErrorList = errorList ?? [] });

    /// <summary> 200 OK — Confirm（需使用者確認後再操作） </summary>
    protected OkObjectResult Confirm(string msg = "", object? data = null, List<ResponseErrorDataViewModel>? errorList = null)
        => Ok(new ResponseViewModel<object> { Status = ResultType.Confirm, Message = msg, Data = data, ErrorList = errorList ?? [] });

    /// <summary> 401 Unauthorized — Fail </summary>
    protected UnauthorizedObjectResult Unauthorized(string msg)
        => Unauthorized(new ResponseViewModel<object> { Status = ResultType.Fail, Message = msg });
}
