import { ResultType } from '~/types/enum'
import type { 
  ResponseViewModel, 
  AuthSessionViewModel, 
  CaptchaViewModel, 
  LoginFailViewModel, 
  LoginRequestViewModel 
} from '~/types/viewmodel'

/**
 * 登入 / 登出 / Session 管理 composable
 *
 * 狀態透過 useState 保存，整個 app 共享同一份。
 */
export const useAuth = () => {
  const api = useApi()

  /** 目前登入的 session（null = 未登入） */
  const session = useState<AuthSessionViewModel | null>('auth:session', () => null)

  /** 是否已登入 */
  const isLoggedIn = computed(() => session.value !== null)

  /** 當前使用者 */
  const user = computed(() => session.value?.User ?? null)

  /** 權限清單 */
  const permissions = computed(() => session.value?.Permissions ?? [])

  // ── 取得圖形驗證碼 ──

  async function fetchCaptcha(): Promise<CaptchaViewModel | null> {
    try {
      const res = await api.get<CaptchaViewModel>('/api/auth/captcha')
      return res.Status === ResultType.Success.Value ? res.Data : null
    }
    catch {
      return null
    }
  }

  // ── 登入 ──

  async function login(payload: LoginRequestViewModel): Promise<ResponseViewModel<LoginFailViewModel>> {
    const res = await api.post<LoginFailViewModel>('/api/auth/login', payload)

    if (res.Status === ResultType.Success.Value) {
      // 登入成功 → Data 是 AuthSessionViewModel
      session.value = res.Data as unknown as AuthSessionViewModel
    }

    return res
  }

  // ── 登出 ──

  async function logout(): Promise<void> {
    try {
      await api.post('/api/auth/logout')
    }
    catch {
      // 即使 API 失敗也清除本地 session
    }
    session.value = null
    await navigateTo('/backend/login')
  }

  // ── 取得 Session（/me） ──

  async function fetchSession(): Promise<boolean> {
    try {
      const res = await api.get<AuthSessionViewModel>('/api/auth/me')
      if (res.Status === ResultType.Success.Value && res.Data) {
        session.value = res.Data
        return true
      }
    }
    catch {
      // token 過期或無效
    }
    session.value = null
    return false
  }

  // ── 刷新 Token ──

  async function refresh(): Promise<boolean> {
    try {
      const res = await api.post<AuthSessionViewModel>('/api/auth/refresh')
      if (res.Status === ResultType.Success.Value && res.Data) {
        session.value = res.Data
        return true
      }
    }
    catch {
      // refresh 失敗
    }
    return false
  }

  return {
    session,
    isLoggedIn,
    user,
    permissions,
    fetchCaptcha,
    login,
    logout,
    fetchSession,
    refresh,
  }
}
