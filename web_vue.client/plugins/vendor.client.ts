/**
 * 第三方套件 CSS 統一在此 import
 *
 * 使用 .client.ts 後綴讓 Nuxt 只在瀏覽器端載入，
 * 避免 Windows 上 Vite @fs 路徑 bug 產生的 404。
 *
 * 新增套件時在此加一行 import 即可，例如：
 *   import 'primevue/resources/themes/aura-light-blue/theme.css'
 *   import 'primevue/resources/primevue.min.css'
 *   import 'primeicons/primeicons.css'
 */

import 'sweetalert2/dist/sweetalert2.min.css'

export default defineNuxtPlugin(() => {})
