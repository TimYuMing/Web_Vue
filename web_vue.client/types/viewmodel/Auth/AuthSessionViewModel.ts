// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
/** 登入 session 資料（登入成功 / me / refresh 共用） */
export interface AuthSessionViewModel {
  /** 使用者基本資訊 */
  User: AuthUserViewModel
  /** 使用者擁有的權限代碼清單 */
  Permissions: string[]
  /** Session 過期時間 (UTC) */
  ExpiresAt: string | null
}

/** 使用者基本資訊 */
export interface AuthUserViewModel {
  /** 使用者 ID */
  Id: number
  /** 帳號 */
  Account: string
  /** 姓名 */
  Name: string
  /** 是否為管理者 */
  IsAdmin: boolean
  /** 系統是否維護中 */
  IsMaintaining: boolean
}
