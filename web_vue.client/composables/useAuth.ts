import type { ResponseModel } from '~/types/api'
import { ResultType } from '~/types/api'
import type { AuthSession, CaptchaModel, LoginFailData } from '~/types/auth'

/**
 * 登入 / 登出 / Session 管理 composable
 *
 * 狀態透過 useState 保存，整個 app 共享同一份。
 */
export const useAuth = () => {
  const api = useApi()

  /** 目前登入的 session（null = 未登入） */
  const session = useState<AuthSession | null>('auth:session', () => null)

  /** 是否已登入 */
  const isLoggedIn = computed(() => session.value !== null)

  /** 當前使用者 */
  const user = computed(() => session.value?.user ?? null)

  /** 權限清單 */
  const permissions = computed(() => session.value?.permissions ?? [])

  // ── 取得圖形驗證碼 ──

  async function fetchCaptcha(): Promise<CaptchaModel | null> {
    try {
      const res = await api.get<CaptchaModel>('/api/auth/captcha')
      return res.status === ResultType.Success ? res.data : null
    }
    catch {
      return null
    }
  }

  // ── 登入 ──

  async function login(payload: {
    account: string
    password: string
    challengeId: string
    validCode: string
  }): Promise<ResponseModel<LoginFailData>> {
    const res = await api.post<LoginFailData>('/api/auth/login', payload)

    if (res.status === ResultType.Success) {
      // 登入成功 → data 是 AuthSession
      session.value = res.data as unknown as AuthSession
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
      const res = await api.get<AuthSession>('/api/auth/me')
      if (res.status === ResultType.Success && res.data) {
        session.value = res.data
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
      const res = await api.post<AuthSession>('/api/auth/refresh')
      if (res.status === ResultType.Success && res.data) {
        session.value = res.data
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
