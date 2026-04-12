using Microsoft.Extensions.Localization;

namespace Web_Vue.Server;

/// <summary> 共用語系檔 </summary>
public class SharedResource
{
    /// <summary> Localizer </summary>
    protected readonly IStringLocalizer Localizer;

    /// <summary> Constructor </summary>
    /// <param name="localizer"></param>
    public SharedResource(IStringLocalizer<SharedResource> localizer)
    {
        Localizer = localizer;
    }
}
