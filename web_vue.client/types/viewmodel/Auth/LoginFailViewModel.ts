// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
import type { LoginFailTypeValue } from '../enum'

/** 登入失敗的附加資訊（放在 ResponseViewModel.Data） */
export interface LoginFailViewModel {
  /** 失敗類型 */
  FailType: LoginFailTypeValue
  /** 建議導向路徑（例如密碼過期時導向 /forget-password） */
  RedirectPath: string | null
}
