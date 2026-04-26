<script setup lang="ts">
  import type { ResponseViewModel } from '~/types/viewmodel'
  import { ResultType } from '~/types/enum'

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

  // ── API 測試區塊 ──────────────────────────────────
  const { post } = useApi()

  const testName = ref('')
  const loading = ref(false)
  const result = ref<ResponseViewModel<{ echo: string; timestamp: string }> | null>(null)

  async function handleTestPost() {
    loading.value = true
    result.value = null
    try {
      result.value = await post('/api/test', { Name: testName.value })
    } finally {
      loading.value = false
    }
  }

  function clickTest() {
    console.log(456)
  }

</script>

<template>
  <div class="front-container">

    <!-- Hero 區塊 -->
    <section class="hero">
      <h1 class="hero-title">{{ $resx('Txt_歡迎標題') }}</h1>
      <p class="hero-desc">{{ $resx('Txt_歡迎描述') }}</p>
      <div class="hero-actions">
        <button class="front-btn front-btn-primary">{{ $resx('Txt_開始探索') }}</button>
        <button class="front-btn front-btn-outline">{{ $resx('Txt_了解更多') }}</button>
      </div>
    </section>

    <hr class="front-divider" />

    <!-- API 測試區塊 -->
    <section class="api-test">
      <h2 class="front-section-title">API 測試（POST /api/test）</h2>
      <div class="api-test-form">
        <input v-model="testName"
               type="text"
               placeholder="輸入 Name 欄位"
               class="api-test-input" />
        <button class="front-btn front-btn-primary"
                :disabled="loading"
                @click="handleTestPost">
          {{ loading ? '傳送中...' : '送出' }}
        </button>

        <button class="front-btn front-btn-primary" @click="clickTest">test</button>
      </div>

      <!-- 回應結果 -->
      <div v-if="result" class="api-test-result" :class="result.Status === ResultType.Success.Value ? 'is-success' : 'is-fail'">
        <p><strong>Status：</strong>{{ result.Status === ResultType.Success.Value ? 'Success' : 'Fail' }}</p>
        <p><strong>Message：</strong>{{ result.Message }}</p>
        <template v-if="result.Status === ResultType.Success.Value && result.Data">
          <p><strong>Echo：</strong>{{ result.Data.echo }}</p>
          <p><strong>Timestamp：</strong>{{ result.Data.timestamp }}</p>
        </template>
        <template v-if="result.ErrorList?.length">
          <p><strong>Errors：</strong></p>
          <ul>
            <li v-for="err in result.ErrorList" :key="err.Key">
              {{ err.Key }}：{{ err.ErrorTextList.join(', ') }}
            </li>
          </ul>
        </template>
      </div>
    </section>

    <hr class="front-divider" />

    <!-- 特色卡片區 -->
    <section class="features">
      <h2 class="front-section-title" style="text-align: center">{{ $resx('Txt_我們的特色') }}</h2>
      <div class="features-grid">
        <div class="front-card feature-card">
          <div class="feature-icon">🚀</div>
          <h3>{{ $resx('Txt_高效能') }}</h3>
          <p>{{ $resx('Txt_高效能描述') }}</p>
        </div>
        <div class="front-card feature-card">
          <div class="feature-icon">🎨</div>
          <h3>{{ $resx('Txt_現代設計') }}</h3>
          <p>{{ $resx('Txt_現代設計描述') }}</p>
        </div>
        <div class="front-card feature-card">
          <div class="feature-icon">🔒</div>
          <h3>{{ $resx('Txt_安全可靠') }}</h3>
          <p>{{ $resx('Txt_安全可靠描述') }}</p>
        </div>
      </div>
    </section>
  </div>
</template>

<style src="~/assets/css/pages/front/home/index.css" scoped />

<style scoped>
  .api-test {
    max-width: 560px;
    margin: 0 auto 2rem;
  }

  .api-test-form {
    display: flex;
    gap: 0.75rem;
    margin-top: 1rem;
  }

  .api-test-input {
    flex: 1;
    padding: 0.5rem 0.75rem;
    border: 1px solid var(--color-border, #ccc);
    border-radius: 6px;
    font-size: 0.95rem;
  }

  .api-test-result {
    margin-top: 1.25rem;
    padding: 1rem 1.25rem;
    border-radius: 8px;
    font-size: 0.9rem;
    line-height: 1.8;
  }

    .api-test-result.is-success {
      background: #f0fdf4;
      border: 1px solid #86efac;
      color: #166534;
    }

    .api-test-result.is-fail {
      background: #fef2f2;
      border: 1px solid #fca5a5;
      color: #991b1b;
    }
</style>
