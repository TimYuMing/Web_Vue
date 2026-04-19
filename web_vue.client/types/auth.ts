import type { ResponseErrorData } from './api'

/** 登入圖形驗證碼 */
export interface CaptchaModel {
  challengeId: string
  imageBase64: string
  expiresAt: string
}

/** 登入成功後的 session 資料 */
export interface AuthSession {
  user: AuthUser
  permissions: string[]
  expiresAt: string | null
}

/** 使用者基本資訊 */
export interface AuthUser {
  id: number
  account: string
  name: string
  isAdmin: boolean
  isMaintaining: boolean
}

/** 登入失敗附加資料 */
export interface LoginFailData {
  failType: LoginFailType
  redirectPath: string | null
}

/** 登入失敗類型 */
export enum LoginFailType {
  驗證失敗 = 1,
  密碼過期 = 2,
  使用暫時密碼 = 3,
}
