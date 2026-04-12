<script setup>
  definePageMeta({
    layout: 'backend',
    title: '儀表板',
  })

  useHead({ title: '儀表板' })

  const $resx = useResx()

  // 統計卡片資料（實際使用時可從 API 取得）
  const stats = computed(() => [
    { label: $resx('Txt_總用戶數'), value: '0', icon: '👥', badge: 'info' },
    { label: $resx('Txt_今日訪問'), value: '0', icon: '📈', badge: 'success' },
    { label: $resx('Txt_待處理項目'), value: '0', icon: '📋', badge: 'warning' },
    { label: $resx('Txt_系統警告'), value: '0', icon: '⚠️', badge: 'danger' },
  ])
</script>

<template>
  <div>
    <h1 class="backend-page-heading">{{ $resx('Txt_儀表板') }}</h1>

    <!-- 統計卡片 -->
    <div class="stats-grid">
      <div v-for="stat in stats"
           :key="stat.label"
           class="backend-card stat-card">
        <div class="stat-header">
          <span class="stat-icon" aria-hidden="true">{{ stat.icon }}</span>
          <span :class="`backend-badge backend-badge-${stat.badge}`">{{ $resx('Txt_即時') }}</span>
        </div>
        <p class="stat-value">{{ stat.value }}</p>
        <p class="stat-label">{{ stat.label }}</p>
      </div>
    </div>

    <!-- 快速操作 -->
    <div class="quick-section">
      <h2 class="backend-section-title">{{ $resx('Txt_快速操作') }}</h2>
      <div class="quick-actions">
        <button class="backend-btn backend-btn-primary">{{ $resx('Txt_新增資料') }}</button>
        <button class="backend-btn backend-btn-outline">{{ $resx('Txt_匯出報表') }}</button>
      </div>
    </div>
  </div>
</template>

<style src="~/assets/css/pages/backend/home/index.css" scoped></style>
