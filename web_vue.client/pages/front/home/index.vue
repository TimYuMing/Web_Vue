<script setup>
  definePageMeta({
    layout: 'front',
    title: '首頁',
  })

  // 頁面層級的 SEO — 會疊加並覆寫 layout 的全域預設值
  useSeoMeta({
    title: '首頁',
    description: '歡迎來到 MySite 首頁，探索我們提供的優質內容與服務。',
    ogTitle: 'MySite | 首頁',
    ogDescription: '歡迎來到 MySite 首頁，探索我們提供的優質內容與服務。',
    ogUrl: 'https://example.com/front/home',
  })
</script>

<template>
  <div class="front-container">
    <!-- Hero 區塊 -->
    <section class="hero">
      <h1 class="hero-title">歡迎來到 MySite</h1>
      <p class="hero-desc">這裡是前台首頁，開始建構你的精彩內容。</p>
      <div class="hero-actions">
        <button class="front-btn front-btn-primary">開始探索</button>
        <button class="front-btn front-btn-outline">了解更多</button>
      </div>
    </section>

    <hr class="front-divider" />

    <!-- 特色卡片區 -->
    <section class="features">
      <h2 class="front-section-title" style="text-align: center">我們的特色</h2>
      <div class="features-grid">
        <div class="front-card feature-card">
          <div class="feature-icon">🚀</div>
          <h3>高效能</h3>
          <p>採用 Nuxt SSR 確保最佳載入速度與 SEO 表現。</p>
        </div>
        <div class="front-card feature-card">
          <div class="feature-icon">🎨</div>
          <h3>現代設計</h3>
          <p>簡潔直觀的介面，帶給使用者最佳體驗。</p>
        </div>
        <div class="front-card feature-card">
          <div class="feature-icon">🔒</div>
          <h3>安全可靠</h3>
          <p>完善的前後端架構，保障資料安全。</p>
        </div>
      </div>
    </section>
  </div>
</template>

<style src="~/assets/css/pages/front/home/index.css" scoped />
