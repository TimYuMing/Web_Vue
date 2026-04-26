// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
/** 登入圖形驗證碼 ViewModel */
export interface CaptchaViewModel {
  /** Challenge ID（呼叫登入 API 時需一併帶入） */
  ChallengeId: string
  /** SVG 驗證碼圖片 data URI (data:image/svg+xml;base64,...) */
  ImageBase64: string
  /** 過期時間 (UTC) */
  ExpiresAt: string
}
