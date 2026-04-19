/**
 * 後台頁面認證守衛
 * — 進入 /backend/** 路由（login 除外）時，若未登入則導向登入頁
 * — 進入 /backend/login 路由時，若已登入則導向後台首頁
 */
export default defineNuxtRouteMiddleware(async (to) => {
  // 只處理後台路徑
  if (!to.path.startsWith('/backend')) return

  const { isLoggedIn, fetchSession } = useAuth()

  // SSR 環境不做 auth check（Cookie 無法直接在 SSR 傳遞至 API）
  if (import.meta.server) return

  // 如果還沒有 session（例如瀏覽器重整），嘗試從 /me 恢復
  if (!isLoggedIn.value) {
    await fetchSession()
  }

  const isLoginPage = to.path === '/backend/login'

  if (isLoginPage && isLoggedIn.value) {
    // 已登入 → 導向後台首頁
    return navigateTo('/backend/home')
  }

  if (!isLoginPage && !isLoggedIn.value) {
    // 未登入 → 導向登入頁
    return navigateTo('/backend/login')
  }
})
