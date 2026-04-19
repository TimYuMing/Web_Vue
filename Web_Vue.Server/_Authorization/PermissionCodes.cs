using Web_Vue.Server._Attribute;

namespace Web_Vue.Server._Authorization;

/// <summary> 權限代碼 </summary>
public static class PermissionCodes
{
    /// <summary> 前台 相關權限 </summary>
    [Permission("前台", 0, isFolder: true, isDisplay: false, isMenu: false)]
    public static class Front
    {
        /// <summary> 首頁 相關權限 </summary>
        [Permission("首頁", 1, "/index", isFolder: true, isMenu: false)]
        public static class Home
        {
            /// <summary> 首頁  權限 </summary>
            [Permission("首頁", 1, "/index")]
            public const string Index = "39408d1e-ada8-4ae6-9bf8-cb988a7d5c71";
        }
    }

    /// <summary> 後台 相關權限 </summary>
    [Permission("後台", 1, isFolder: true, isDisplay: false, isMenu: false)]
    public static class Backend
    {
        /// <summary> 凌誠 相關權限 </summary>
        [Permission("凌誠", 0, isFolder: true, isDisplay: false, isMenu: false)]
        public static class Linkchain
        {
            /// <summary> 功能頁 權限 </summary>
            [Permission("功能頁", 1, "/linkchain", isDisplay: false)]
            public const string LinkchainIndex = "52813caf-f8e5-4632-9e32-6d8c55741e59";

            /// <summary> 權限掃描 權限 </summary>
            [Permission("權限掃描", 2, "/linkchain/permission-scanning", isDisplay: false)]
            public const string LinkchainPermissionScanning = "df63d067-c070-49a5-a6f1-ec4dbc311515";

            /// <summary> 錯誤紀錄列表 權限 </summary>
            [Permission("錯誤紀錄", 3, "/linkchain/error-log", isDisplay: false)]
            public const string ErrorLogList = "c27c21f0-e81a-45ec-8ae7-da6b79c4c4a6";

            /// <summary> 系統參數列表 權限 </summary>
            [Permission("系統參數", 4, "/linkchain/system-config", isDisplay: false)]
            public const string SystemConfigList = "dacfa0ca-f812-4698-8188-0945ef0afeb9";

            /// <summary> 套件範本 權限 </summary>
            [Permission("套件範本", 5, "/linkchain/linkchain/package-template", isDisplay: false)]
            public const string PackageTemplateList = "afb9de2b-1784-4b9f-aecc-f110abae2ec3";
        }

        /// <summary> 首頁 相關權限 </summary>
        [Permission("首頁", 1, "/index", isFolder: true, isMenu: false)]
        public static class Home
        {
            /// <summary> 首頁看板  權限 </summary>
            [Permission("首頁看板", 1, "/index")]
            public const string Index = "270a86d9-348e-4542-ae29-85430520350f";
        }
    }

}
