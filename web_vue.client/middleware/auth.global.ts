/**
 * 後台頁面認證守衛
 * — 進入 /backend/** 路由（login 除外）時，若未登入則導向登入頁
 * — 進入 /backend/login 路由時，若已登入則導向後台首頁
 */
export default defineNuxtRouteMiddleware(async (to) => {
  // 只處理後台路徑
  if (!to.path.startsWith('/backend')) {
    return
  }

  const { isLoggedIn, fetchSession } = useAuth()
  const isLoginPage = to.path === '/backend/login'

  // 只有在尚未有 session 時才呼叫 /me 嘗試恢復：
  // - 受保護頁面：必定要嘗試（重整後 session 記憶體會清空）
  // - login 頁面：不需要，未登入必然 401 只是噪音；若快取中已有 session 則下方重導即可
  if (!isLoggedIn.value && !isLoginPage) {
    await fetchSession()
  }

  if (isLoginPage && isLoggedIn.value) {
    // 已登入 → 導向後台首頁
    return navigateTo('/backend/home')
  }

  if (!isLoginPage && !isLoggedIn.value) {
    // 未登入 → 導向登入頁
    return navigateTo('/backend/login')
  }
})
