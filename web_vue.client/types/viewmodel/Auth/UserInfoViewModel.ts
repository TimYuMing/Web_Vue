// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
import type { UserProfileStatusValue } from '../enum'

/** 使用者資料模型（序列化後存入 JWT Claims） */
export interface UserInfoViewModel {
  /** 使用者 ID */
  ID: number
  /** 帳號 */
  Account: string
  /** 姓名 */
  Name: string
  /** 帳號狀態 */
  Status: UserProfileStatusValue
  /** 最後登入日期 */
  LatestLoginDate: string | null
  /** 為暫時密碼 */
  IsTemplatePassword: boolean
  /** 暫時密碼驗證有效期 */
  ValidPasswordExpireTime: string | null
  /** 所屬角色 ID 清單 */
  RoleList: number[]
  /** 是否為管理者 */
  IsAdmin: boolean
  /** 角色是否系統維護中 */
  IsMaintaining: boolean
}
