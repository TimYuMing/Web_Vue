namespace Web_Vue.Server._Attribute;

/// <summary> 功能權限 屬性 </summary>
public class PermissionAttribute : Attribute
{
    /// <summary> 功能名稱 </summary>
    public string Name { get; private set; }

    /// <summary> 排序 </summary>
    public int Sort { get; private set; }

    /// <summary> 功能路徑 </summary>
    public string Url { get; private set; }

    /// <summary> 是否為資料夾 </summary>
    public bool IsFolder { get; private set; }

    /// <summary> 父節點代碼 </summary>
    public string ParentCode { get; private set; }

    /// <summary> 是否出現於Menu選單 </summary>
    public bool IsMenu { get; set; }

    /// <summary> 是否顯示於權限清單上 </summary>
    public bool IsDisplay { get; private set; }

    /// <summary> Constructor </summary>
    /// <param name="name"> 功能名稱 </param>
    /// <param name="sort"> 排序 </param>
    /// <param name="url"> 功能路徑 </param>
    /// <param name="isFolder"> 是否為資料夾 </param>
    /// <param name="parentCode"> 父節點代碼 </param>
    /// <param name="isMenu"> 是否出現於Menu選單 </param>
    /// <param name="isDisplay"> 是否顯示於權限清單上 </param>
    public PermissionAttribute(string name, int sort, string url = "", bool isFolder = false, string parentCode = "", bool isMenu = true, bool isDisplay = true)
    {
        Name = name;
        Sort = sort;
        Url = string.IsNullOrWhiteSpace(url) ? "#" : url;
        IsFolder = isFolder;
        ParentCode = parentCode;
        IsMenu = isMenu;
        IsDisplay = isDisplay;
    }
}
