// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
/** 登入請求 DTO */
export interface LoginRequestViewModel {
  /** 使用者帳號 */
  Account: string
  /** 密碼 */
  Password: string
  /** 驗證碼 Challenge ID（由 /api/auth/captcha 取得） */
  ChallengeId: string
  /** 使用者輸入的圖形驗證碼 */
  ValidCode: string
}
