// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
import type { ResultTypeValue } from '../enum'

/** 統一 API 回應模型 */
export interface ResponseViewModel<T = unknown> {
  /** 回應結果狀態 */
  Status: ResultTypeValue
  /** 回應資料內容 */
  Data: T | null
  /** 回應訊息 */
  Message: string
  /** 錯誤清單 */
  ErrorList: ResponseErrorDataViewModel[]
}

/** 回應錯誤資料模型 */
export interface ResponseErrorDataViewModel {
  /** 鍵 */
  Key: string
  /** 錯誤文字清單 */
  ErrorTextList: string[]
}
