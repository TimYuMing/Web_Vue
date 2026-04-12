<script setup>
  // locale / locales 為響應式 Ref，需在 setup 中透過 useI18n() 取得
  const { locale, locales } = useI18n()

  const $resx = useResx()

  // 前台全域 SEO 預設值 — 各頁面可透過 useSeoMeta / useHead 覆寫
  useSeoMeta({
    titleTemplate: (title) => title ? `${title} | MySite` : 'MySite',
    description: '歡迎來到 MySite，提供優質的內容與服務。',
    ogType: 'website',
    ogSiteName: 'MySite',
    ogImage: '/og-image.png',
    twitterCard: 'summary_large_image',
    robots: 'index, follow',
  })

  useHead({
    htmlAttrs: { lang: 'zh-TW' },
    // canonical 可由各頁面覆寫
    link: [{ rel: 'canonical', href: useRequestURL().href }],
  })

  // 導覽選單項目（使用 computed 確保語系切換時即時更新）
  const navItems = computed(() => [
    { label: $resx('Txt_首頁'), path: '/front/home' },
    // 未來可在此新增更多導覽項目
  ])
</script>

<template>
  <div class="front-layout">
    <!-- 導覽列 -->
    <header class="front-header">
      <div class="front-container">
        <nav class="front-nav" aria-label="主選單">
          <NuxtLink to="/front/home" class="front-logo" aria-label="回首頁">MySite</NuxtLink>

          <ul class="front-nav-links" role="list">
            <li v-for="item in navItems" :key="item.path">
              <NuxtLink :to="item.path" :aria-label="item.label">{{ item.label }}</NuxtLink>
            </li>
          </ul>

          <!-- 語系切換 -->
          <div class="front-lang-switcher">
            <button
              v-for="loc in locales"
              :key="loc.code"
              class="front-lang-btn"
              :class="{ 'front-lang-btn--active': locale === loc.code }"
              @click="$setLocale(loc.code)"
            >
              {{ loc.name }}
            </button>
          </div>
        </nav>
      </div>
    </header>

    <!-- 主要內容 -->
    <main class="front-main" id="main-content">
      <slot />
    </main>

    <!-- 頁腳 -->
    <footer class="front-footer" aria-label="頁腳">
      <div class="front-container">
        <p>&copy; {{ new Date().getFullYear() }} MySite. All rights reserved.</p>
      </div>
    </footer>
  </div>
</template>

<!-- 載入前台全域 CSS（不加 scoped，確保 CSS 變數與全域類別可用） -->
<style>
  @import '~/assets/css/layouts/front.css';
</style>

<!-- 版型結構樣式（獨立管理於 assets/css/layouts/front.scoped.css） -->
<style src="~/assets/css/layouts/front.scoped.css" scoped />
