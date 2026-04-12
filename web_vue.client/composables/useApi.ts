import type { ResponseModel } from '~/types/api'

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
 *   get(url, query?)   直接回傳 Promise<ResponseModel<T>>
 *   post(url, body?)   同上
 */
export const useApi = () => {
  // 惰性取得語系：每次請求時才讀取，避免 setup 初始化時 $i18n 尚未就緒導致 hydration 失敗
  const getLocale = (): string => {
    try { return (useNuxtApp().$i18n as any).locale.value ?? 'zh-TW' }
    catch { return 'zh-TW' }
  }

  // ── SSR 系列 ────────────────────────────────────────────────────────

  const ssrGet = <T = unknown>(key: string, url: string, query?: Record<string, unknown>) =>
    useFetch<ResponseModel<T>>(url, {
      key,
      method: 'GET',
      query,
      headers: { 'Accept-Language': getLocale() },
    })

  const ssrPost = <T = unknown>(key: string, url: string, body?: unknown) =>
    useFetch<ResponseModel<T>>(url, {
      key,
      method: 'POST',
      body,
      server: false,
      immediate: false,
      onRequest({ options }) {
        options.headers = {
          ...(options.headers as Record<string, string>),
          'Accept-Language': getLocale(),
        }
      },
    })

  // ── 事件處理器系列 ───────────────────────────────────────────────────

  const get = <T = unknown>(url: string, query?: Record<string, unknown>) =>
    $fetch<ResponseModel<T>>(url, {
      method: 'GET',
      query,
      headers: { 'Accept-Language': getLocale() },
    })

  const post = <T = unknown>(url: string, body?: unknown) =>
    $fetch<ResponseModel<T>>(url, {
      method: 'POST',
      body,
      headers: { 'Accept-Language': getLocale() },
    })

  return { ssrGet, ssrPost, get, post }
}
