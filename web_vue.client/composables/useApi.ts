import type { ResponseViewModel } from '~/types/viewmodel'

/**
 * 共用 API composable（Nuxt auto-import，無需手動引入）
 *
 * ─ SSR 系列（useFetch 包裝，在 <script setup> 頂層呼叫）────────────────
 *   ssrGet(key, url, query?)
 *     支援 SSR hydration，回傳 { data, error, pending, refresh }
 *     適用：頁面初始資料載入
 *
 *   ssrPost(key, url, body?)
 *     僅在 client 執行，回傳 { data, error, pending, execute }
 *     適用：需要響應式 loading/error 狀態的表單提交
 *     用法：const { data, execute } = useApi().ssrPost('key', '/api/test', body)
 *           await execute()   // 在 click handler 中觸發
 *
 * ─ 事件處理器系列（$fetch 包裝，在 function / async handler 中呼叫）──────
 *   get(url, query?)   直接回傳 Promise<ResponseViewModel<T>>
 *   post(url, body?)   同上
 */
export const useApi = () => {
  // 惰性取得語系：每次請求時才讀取，避免 setup 初始化時 $i18n 尚未就緒導致 hydration 失敗
  const getLocale = (): string => {
    try { return (useNuxtApp().$i18n as any).locale.value ?? 'zh-TW' }
    catch { return 'zh-TW' }
  }

  // 從 Cookie 讀取 CSRF Token（登入後由後端設定）
  const getCsrfToken = (): string => {
    if (import.meta.server) return ''
    // Config.CsrfCookieName = "csrf-token"
    const match = document.cookie.match(/(?:^|;\s*)csrf-token=([^;]*)/)
    return match ? decodeURIComponent(match[1]) : ''
  }

  // 共用 headers
  const buildHeaders = (): Record<string, string> => {
    const headers: Record<string, string> = {
      'Accept-Language': getLocale(),
    }

    if (import.meta.server) {
      // SSR：把瀏覽器的 Cookie header 轉發給後端，讓 JWT 驗證能運作
      // GET 請求已在 CSRF middleware 豈免，安全
      const reqHeaders = useRequestHeaders(['cookie'])
      if (reqHeaders.cookie) {
        headers['cookie'] = reqHeaders.cookie
      }
    } else {
      // Client：加入 CSRF token
      const csrf = getCsrfToken()
      if (csrf) {
        headers['X-CSRF-Token'] = csrf
      }
    }

    return headers
  }

  // ── SSR 系列 ────────────────────────────────────────────────────────

  const ssrGet = <T = unknown>(key: string, url: string, query?: Record<string, unknown>) =>
    useFetch<ResponseViewModel<T>>(url, {
      key,
      method: 'GET',
      query,
      headers: buildHeaders(),
    })

  const ssrPost = <T = unknown>(key: string, url: string, body?: BodyInit | Record<string, any> | null) =>
    useFetch<ResponseViewModel<T>>(url, {
      key,
      method: 'POST',
      body,
      server: false,
      immediate: false,
      onRequest({ options }) {
        const existing = options.headers instanceof Headers
          ? Object.fromEntries(options.headers.entries())
          : (options.headers as Record<string, string> | undefined) ?? {}
        options.headers = new Headers({ ...existing, ...buildHeaders() })
      },
    })

  // ── 事件處理器系列 ───────────────────────────────────────────────────

  const get = <T = unknown>(url: string, query?: Record<string, unknown>) =>
    $fetch<ResponseViewModel<T>>(url, {
      method: 'GET',
      query,
      headers: buildHeaders(),
    })

  const post = <T = unknown>(url: string, body?: BodyInit | Record<string, any> | null) =>
    $fetch<ResponseViewModel<T>>(url, {
      method: 'POST',
      body,
      headers: buildHeaders(),
    })

  return { ssrGet, ssrPost, get, post }
}
