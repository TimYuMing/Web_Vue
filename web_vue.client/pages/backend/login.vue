<script setup lang="ts">
import { ResultType } from '~/types/enum'

definePageMeta({
  layout: false,
  title: '登入',
})

useHead({ title: '登入 | 管理後台' })

const $resx = useResx()
const { fetchCaptcha, login } = useAuth()

// 表單資料
const form = reactive({
  account: '',
  password: '',
  validCode: '',
})

// 驗證碼狀態
const captchaImage = ref('')
const challengeId = ref('')
const isLoading = ref(false)
const errorMessage = ref('')
const fieldErrors = ref<Record<string, string[]>>({})

// 載入驗證碼
async function loadCaptcha() {
  const captcha = await fetchCaptcha()
  if (captcha) {
    captchaImage.value = captcha.ImageBase64
    challengeId.value = captcha.ChallengeId
  }
}

// 初始載入
onMounted(() => loadCaptcha())

async function handleLogin() {
  errorMessage.value = ''
  fieldErrors.value = {}

  if (!form.account || !form.password || !form.validCode) {
    errorMessage.value = '請填寫所有欄位'
    return
  }

  isLoading.value = true
  try {
    const res = await login({
      Account: form.account,
      Password: form.password,
      ChallengeId: challengeId.value,
      ValidCode: form.validCode,
    })

    if (res.Status === ResultType.Success.Value) {
      // 登入成功 → middleware 會讓 /backend/home 通過
      await navigateTo('/backend/home')
      return
    }

    // 登入失敗
    if (res.ErrorList?.length) {
      for (const err of res.ErrorList) {
        fieldErrors.value[err.Key] = err.ErrorTextList
      }
    }

    if (res.Message) {
      errorMessage.value = res.Message
    }

    const failData = res.Data
    if (failData?.RedirectPath) {
      errorMessage.value = res.Message || '請依指示操作'
      // 密碼過期 / 暫時密碼 → 導向特定頁面（未來可擴充）
    }
  }
  catch {
    errorMessage.value = '登入失敗，請稍後再試'
  }
  finally {
    isLoading.value = false
    // 刷新驗證碼（不論成功與否，challenge 只能使用一次）
    form.validCode = ''
    await loadCaptcha()
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <!-- 頁首 -->
      <header class="login-header">
        <span class="login-brand">{{ $resx('Txt_後台品牌名稱') }}</span>
        <p class="login-subtitle">{{ $resx('Txt_登入副標題') }}</p>
      </header>

      <!-- 錯誤訊息 -->
      <div v-if="errorMessage" class="login-error" role="alert">
        {{ errorMessage }}
      </div>

      <!-- 登入表單 -->
      <form class="login-form" @submit.prevent="handleLogin" novalidate>
        <!-- 帳號 -->
        <div class="form-group">
          <label class="form-label" for="login-account">{{ $resx('Txt_帳號') }}</label>
          <div class="input-wrapper">
            <span class="input-icon" aria-hidden="true">👤</span>
            <input
              id="login-account"
              v-model="form.account"
              type="text"
              class="form-input"
              :class="{ 'input-error': fieldErrors['Account'] }"
              :placeholder="$resx('Txt_請輸入帳號')"
              autocomplete="username"
              required
            />
          </div>
          <p v-if="fieldErrors['Account']" class="field-error">{{ fieldErrors['Account'][0] }}</p>
        </div>

        <!-- 密碼 -->
        <div class="form-group">
          <label class="form-label" for="login-password">{{ $resx('Txt_密碼') }}</label>
          <div class="input-wrapper">
            <span class="input-icon" aria-hidden="true">🔒</span>
            <input
              id="login-password"
              v-model="form.password"
              type="password"
              class="form-input"
              :class="{ 'input-error': fieldErrors['Password'] }"
              :placeholder="$resx('Txt_請輸入密碼')"
              autocomplete="current-password"
              required
            />
          </div>
          <p v-if="fieldErrors['Password']" class="field-error">{{ fieldErrors['Password'][0] }}</p>
        </div>

        <!-- 驗證碼 -->
        <div class="form-group">
          <label class="form-label" for="login-captcha">{{ $resx('Txt_驗證碼') }}</label>
          <div class="captcha-row">
            <div class="form-group">
              <div class="input-wrapper">
                <span class="input-icon" aria-hidden="true">🔑</span>
                <input
                  id="login-captcha"
                  v-model="form.validCode"
                  type="text"
                  class="form-input"
                  :class="{ 'input-error': fieldErrors['ValidCode'] }"
                  :placeholder="$resx('Txt_請輸入驗證碼')"
                  autocomplete="off"
                  maxlength="6"
                  required
                />
              </div>
            </div>

            <!-- 驗證碼圖片（點擊可刷新） -->
            <div
              class="captcha-image-wrapper"
              :title="$resx('Txt_點擊刷新')"
              role="button"
              tabindex="0"
              :aria-label="$resx('Txt_驗證碼')"
              @click="loadCaptcha"
              @keydown.enter="loadCaptcha"
            >
              <img
                v-if="captchaImage"
                :src="captchaImage"
                alt="驗證碼"
                class="captcha-img"
              />
              <span v-else class="captcha-placeholder-text" aria-hidden="true">載入中…</span>
              <span class="captcha-refresh-hint">{{ $resx('Txt_點擊刷新') }}</span>
            </div>
          </div>
          <p v-if="fieldErrors['ValidCode']" class="field-error">{{ fieldErrors['ValidCode'][0] }}</p>
        </div>

        <!-- 送出 -->
        <button type="submit" class="login-submit" :disabled="isLoading">
          {{ isLoading ? '登入中…' : $resx('Txt_登入按鈕') }}
        </button>
      </form>

      <footer class="login-footer">
        &copy; {{ new Date().getFullYear() }} MySite &mdash; {{ $resx('Txt_版權聲明') }}
      </footer>
    </div>
  </div>
</template>

<style>
/* 載入後台 CSS 變數（不 scoped，讓 --backend-* 變數在此頁可用） */
@import '~/assets/css/layouts/backend.css';
</style>

<style src="~/assets/css/pages/backend/login.css" scoped />
