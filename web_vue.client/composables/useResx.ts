type ResxParams = Record<string, string | number> | (string | number)[]

/**
 * 取得全域 $resx 翻譯函式（Nuxt auto-import，無需手動引入）
 * script setup 用法：const $resx = useResx()
 */
export const useResx = (): ((key: string, params?: ResxParams) => string) => {
  return useNuxtApp().$resx as (key: string, params?: ResxParams) => string
}
