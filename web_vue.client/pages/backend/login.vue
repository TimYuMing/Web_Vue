<script setup>
definePageMeta({
  layout: false, // 登入頁不套用後台 layout（無側欄）
  title: '登入',
})

useHead({ title: '登入 | 管理後台' })

// 表單資料
const form = reactive({
  account: '',
  password: '',
  captcha: '',
})

// 示意用驗證碼（之後串接後端 API 產生）
const mockCaptchaCode = ref('8K3mZ')

function refreshCaptcha() {
  // TODO: 呼叫後端 API 重新取得驗證碼圖片
  const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789'
  mockCaptchaCode.value = Array.from(
    { length: 5 },
    () => chars[Math.floor(Math.random() * chars.length)],
  ).join('')
}

function handleLogin() {
  // TODO: 呼叫後端登入 API
  console.log('登入資料：', form)
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <!-- 頁首 -->
      <header class="login-header">
        <span class="login-brand">backend</span>
        <p class="login-subtitle">請登入以進入管理後台</p>
      </header>

      <!-- 登入表單 -->
      <form class="login-form" @submit.prevent="handleLogin" novalidate>
        <!-- 帳號 -->
        <div class="form-group">
          <label class="form-label" for="login-account">帳號</label>
          <div class="input-wrapper">
            <span class="input-icon" aria-hidden="true">👤</span>
            <input
              id="login-account"
              v-model="form.account"
              type="text"
              class="form-input"
              placeholder="請輸入帳號"
              autocomplete="username"
              required
            />
          </div>
        </div>

        <!-- 密碼 -->
        <div class="form-group">
          <label class="form-label" for="login-password">密碼</label>
          <div class="input-wrapper">
            <span class="input-icon" aria-hidden="true">🔒</span>
            <input
              id="login-password"
              v-model="form.password"
              type="password"
              class="form-input"
              placeholder="請輸入密碼"
              autocomplete="current-password"
              required
            />
          </div>
        </div>

        <!-- 驗證碼 -->
        <div class="form-group">
          <label class="form-label" for="login-captcha">驗證碼</label>
          <div class="captcha-row">
            <div class="form-group">
              <div class="input-wrapper">
                <span class="input-icon" aria-hidden="true">🔑</span>
                <input
                  id="login-captcha"
                  v-model="form.captcha"
                  type="text"
                  class="form-input"
                  placeholder="請輸入驗證碼"
                  autocomplete="off"
                  maxlength="6"
                  required
                />
              </div>
            </div>

            <!-- 驗證碼示意圖（點擊可刷新） -->
            <div
              class="captcha-image-wrapper"
              title="點擊刷新驗證碼"
              role="button"
              tabindex="0"
              aria-label="驗證碼圖片，點擊刷新"
              @click="refreshCaptcha"
              @keydown.enter="refreshCaptcha"
            >
              <span class="captcha-placeholder-text" aria-hidden="true">
                {{ mockCaptchaCode }}
              </span>
              <span class="captcha-refresh-hint">點擊刷新</span>
            </div>
          </div>
        </div>

        <!-- 送出 -->
        <button type="submit" class="login-submit">登入</button>
      </form>

      <footer class="login-footer">
        &copy; {{ new Date().getFullYear() }} MySite &mdash; 僅限授權人員使用
      </footer>
    </div>
  </div>
</template>

<style>
/* 載入後台 CSS 變數（不 scoped，讓 --backend-* 變數在此頁可用） */
@import '~/assets/css/layouts/backend.css';
</style>

<style src="~/assets/css/pages/backend/login.css" scoped />
