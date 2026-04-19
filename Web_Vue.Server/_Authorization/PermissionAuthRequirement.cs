namespace Web_Vue.Server._Authorization;

/// <summary> 功能權限需求 </summary>
public class PermissionAuthRequirement : IAuthorizationRequirement
{
    /// <summary> 權限代碼清單 </summary>
    public List<string> PermissionCodeList { get; set; }

    /// <summary> 建構子 </summary>
    public PermissionAuthRequirement(List<string> permissionCodeList)
    {
        PermissionCodeList = permissionCodeList;
    }
}
