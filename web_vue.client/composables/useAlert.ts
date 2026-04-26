/**
 * SweetAlert2 封裝 composable（對應 PSG3 api/alert.js）
 *
 * 僅在 client 端執行，SweetAlert2 以動態 import 載入避免 SSR 問題。
 *
 * 用法：
 *   const { alertKit, confirmKit } = useAlert()
 *   await alertKit({ title: '操作成功', icon: 'success' })
 *   const result = await confirmKit({ title: '確定刪除？' })
 *   if (result.isConfirmed) { ... }
 */

type SwalIcon = 'success' | 'error' | 'warning' | 'info' | 'question'

interface AlertOptions {
  title: string
  icon?: SwalIcon
}

interface ConfirmOptions {
  title: string
  text?: string
  icon?: SwalIcon
  allowOutsideClick?: boolean
}

export const useAlert = () => {
  const $resx = useResx()

  const getSwal = () => import('sweetalert2').then((m) => m.default)

  /**
   * 顯示訊息彈窗（僅確定鍵），對應 PSG3 AlertMessage
   */
  const alertKit = async (options: AlertOptions) => {
    if (import.meta.server) return undefined
    const Swal = await getSwal()
    return Swal.fire({
      icon: options.icon ?? 'warning',
      title: options.title,
      confirmButtonText: $resx('Btn_確定'),
    })
  }

  /**
   * 顯示確認彈窗（確定 + 取消），對應 PSG3 ConfirmMessage
   * 回傳值：{ isConfirmed } 判斷使用者是否按下確認
   */
  const confirmKit = async (options: ConfirmOptions) => {
    if (import.meta.server) return undefined
    const Swal = await getSwal()
    const outsideClick = options.allowOutsideClick ?? true
    return Swal.fire({
      icon: options.icon ?? 'warning',
      title: options.title,
      text: options.text,
      showCancelButton: true,
      confirmButtonText: $resx('Btn_確定'),
      cancelButtonText: $resx('Btn_取消'),
      allowOutsideClick: outsideClick,
      allowEscapeKey: outsideClick,
    })
  }

  return { alertKit, confirmKit }
}
