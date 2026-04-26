using Web_Vue.Server._Authorization;
using Web_Vue.Server.Services;

namespace Web_Vue.Server.Controllers;

/// <summary> Linkchain（凌誠）系統管理 API </summary>
[Route("api/[controller]")]
public class LinkchainController(
    PermissionService permissionService,
    IStringLocalizer<SharedResource> sharedLocalizer) : BaseController
{
    /// <summary> 權限掃描：自動同步 PermissionCodes 定義至資料庫 </summary>
    [HttpPost("permission-scan")]
    [ApiPermissionFilter(PermissionCodes.Backend.Linkchain.LinkchainPermissionScanning)]
    public async Task<IActionResult> PermissionScan()
    {
        await permissionService.ScanningAsync();
        return Success(sharedLocalizer["Txt_權限掃描成功"].Value);
    }
}
