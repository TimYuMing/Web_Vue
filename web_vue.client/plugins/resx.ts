/**
 * resx plugin — 全域語系注入
 * 將翻譯方法透過 Nuxt provide 注入至全域，無需每頁宣告
 *
 * Template 中使用方式（無需任何宣告）：
 *   {{ $resx('Txt_歡迎標題') }}
 *   {{ $resx('ValidMessage_Maxlength', { field: $resx('Txt_欄位'), max: 120 }) }}
 *   <button @click="$setLocale('en-US')">EN</button>
 *
 * Script 中使用方式：
 *   const { $resx, $setLocale } = useNuxtApp()
 *
 * 需要響應式的 locale / locales 狀態時，在 <script setup> 中使用 useI18n()：
 *   const { locale, locales } = useI18n()
 */
export default defineNuxtPlugin((nuxtApp) => {
  // $i18n 由 @nuxtjs/i18n 模組在 module plugin 階段注入，此處可安全存取
  const i18n = nuxtApp.$i18n as any

  /**
   * 取得對應語系的文字，key 對應後端 resx 的 name 屬性
   * @param key    resx 資源 key
   * @param params 格式化參數：
   *               - 物件 { field: '報告抬頭', max: 120 } 對應 value 中的 {field}, {max}（建議）
   *               - 陣列 ['報告抬頭', 120] 對應 value 中的 {0}, {1}（相容用法）
   */
  const resx = (
    key: string,
    params?: Record<string, string | number> | (string | number)[],
  ): string => {
    if (params !== undefined) {
      return i18n.t(key, params)
    }
    return i18n.t(key)
  }

  return {
    provide: {
      /** 取得語系文字，全域可用，無需在頁面宣告 */
      resx,
      /** 切換語系，全域可用 */
      setLocale: (code: string) => i18n.setLocale(code),
    },
  }
})

