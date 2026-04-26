// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
/** 登入成功回應 DTO */
export interface LoginResponseViewModel {
  /** JWT Token */
  Token: string
  /** 過期時間 (分鐘) */
  ExpireMinutes: number
  /** 使用者帳號 */
  Account: string
  /** 姓名 */
  Name: string
  /** 是否為管理者 */
  IsAdmin: boolean
  /** 所屬角色 ID 清單 */
  RoleList: number[]
}
