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
}
