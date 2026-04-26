// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/Enums

/** 回應結果類型 */
export interface ResultTypeMember {
  Value: 1 | 2 | 3
  Text: 'Txt_成功' | 'Txt_失敗' | 'Txt_確認'
  toString(): string
  [Symbol.toPrimitive](hint: string): number | string
}

/** 回應結果類型 */
export const ResultType = Object.freeze({
  /** 成功 */
  Success: Object.freeze({
    Value: 1,
    Text: 'Txt_成功',
    toString() { return String(1) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '1' : 1 },
  }),
  /** 失敗 */
  Fail: Object.freeze({
    Value: 2,
    Text: 'Txt_失敗',
    toString() { return String(2) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '2' : 2 },
  }),
  /** 確認 */
  Confirm: Object.freeze({
    Value: 3,
    Text: 'Txt_確認',
    toString() { return String(3) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '3' : 3 },
  }),
} as const satisfies Record<string, ResultTypeMember>)

export type ResultTypeValue = 1 | 2 | 3

/** 帳號狀態 列舉 */
export interface UserProfileStatusMember {
  Value: 1 | 2
  Text: 'Txt_啟用' | 'Txt_停用'
  toString(): string
  [Symbol.toPrimitive](hint: string): number | string
}

/** 帳號狀態 列舉 */
export const UserProfileStatus = Object.freeze({
  /** 啟用 */
  Enable: Object.freeze({
    Value: 1,
    Text: 'Txt_啟用',
    toString() { return String(1) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '1' : 1 },
  }),
  /** 停用 */
  Disable: Object.freeze({
    Value: 2,
    Text: 'Txt_停用',
    toString() { return String(2) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '2' : 2 },
  }),
} as const satisfies Record<string, UserProfileStatusMember>)

export type UserProfileStatusValue = 1 | 2

/** 登入失敗類型 */
export interface LoginFailTypeMember {
  Value: 1 | 2 | 3
  Text: 'Txt_驗證失敗' | 'Txt_密碼過期' | 'Txt_使用暫時密碼'
  toString(): string
  [Symbol.toPrimitive](hint: string): number | string
}

/** 登入失敗類型 */
export const LoginFailType = Object.freeze({
  /** 驗證失敗（帳密錯誤、帳號停用、驗證碼錯誤等一般失敗） */
  AuthFailed: Object.freeze({
    Value: 1,
    Text: 'Txt_驗證失敗',
    toString() { return String(1) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '1' : 1 },
  }),
  /** 密碼已過期，需重設 */
  PasswordExpired: Object.freeze({
    Value: 2,
    Text: 'Txt_密碼過期',
    toString() { return String(2) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '2' : 2 },
  }),
  /** 使用暫時密碼，需更換密碼 */
  UseTempPassword: Object.freeze({
    Value: 3,
    Text: 'Txt_使用暫時密碼',
    toString() { return String(3) },
    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '3' : 3 },
  }),
} as const satisfies Record<string, LoginFailTypeMember>)

export type LoginFailTypeValue = 1 | 2 | 3
