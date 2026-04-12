using Microsoft.AspNetCore.Mvc;

namespace Web_Vue.Server.Controllers
{
    /// <summary>
    /// API 控制器基底類別，所有控制器皆應繼承此類別
    /// 可在此類別上掛載共用的 Filter、授權、Exception 處理等
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
    }
}
