/**
 * 對應後端 ResultType enum
 */
export enum ResultType {
  Success = 1,
  Fail = 2,
  Confirm = 3,
}

/**
 * 對應後端 ResponseErrorDataViewModel
 */
export interface ResponseErrorData {
  key: string
  errorTextList: string[]
}

/**
 * 對應後端 ResponseModel
 * @template T - Data 欄位的型別
 */
export interface ResponseModel<T = unknown> {
  status: ResultType
  data: T | null
  message: string
  errorList: ResponseErrorData[]
}
