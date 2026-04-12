using System.ComponentModel;

namespace Web_Vue.Server.Enums
{
    /// <summary>
    /// 回應結果類型
    /// </summary>
    public enum ResultType
    {
        /// <summary> 成功 </summary>
        [Description("Txt_成功")]
        Success = 1,

        /// <summary> 失敗 </summary>
        [Description("Txt_失敗")]
        Fail = 2,

        /// <summary> 確認 </summary>
        [Description("Txt_確認")]
        Confirm = 3
    }
}
