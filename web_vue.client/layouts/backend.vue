<script setup>
// 後台不對外開放，禁止搜尋引擎索引
useHead({
  htmlAttrs: { lang: 'zh-TW' },
  meta: [{ name: 'robots', content: 'noindex, nofollow' }],
  titleTemplate: (title) => title ? `${title} | 管理後台` : '管理後台',
})

const route = useRoute()

// 側欄導覽項目  未來可擴充子選單
const navItems = [
  { label: '儀表板', path: '/backend/home', icon: '◈' },
  // 可在此新增更多後台選單項目
]
</script>

<template>
  <div class="backend-layout">
    <!-- 側欄 -->
    <aside class="backend-sidebar" aria-label="後台側欄">
      <div class="backend-sidebar-header">
        <NuxtLink to="/backend/home" class="backend-sidebar-logo">backend</NuxtLink>
      </div>

      <nav class="backend-sidebar-nav" aria-label="後台主選單">
        <ul role="list">
          <li v-for="item in navItems" :key="item.path">
            <NuxtLink
              :to="item.path"
              class="backend-nav-item"
              :class="{ active: route.path === item.path }"
              :aria-current="route.path === item.path ? 'page' : undefined"
            >
              <span class="backend-nav-icon" aria-hidden="true">{{ item.icon }}</span>
              <span>{{ item.label }}</span>
            </NuxtLink>
          </li>
        </ul>
      </nav>
    </aside>

    <!-- 右側主區域 -->
    <div class="backend-body">
      <!-- 頂部列 -->
      <header class="backend-topbar">
        <div class="backend-topbar-left">
          <span class="backend-topbar-title">{{ route.meta.title ?? '管理後台' }}</span>
        </div>
        <div class="backend-topbar-right">
          <span class="backend-user-info">管理員</span>
        </div>
      </header>

      <!-- 頁面內容 -->
      <main class="backend-content" id="backend-main">
        <slot />
      </main>
    </div>
  </div>
</template>

<!-- 載入後台全域 CSS（不加 scoped，確保 CSS 變數與全域類別可用） -->
<style>
@import '~/assets/css/layouts/backend.css';
</style>

<!-- 版型結構樣式（獨立管理於 assets/css/layouts/backend.scoped.css） -->
<style src="~/assets/css/layouts/backend.scoped.css" scoped />