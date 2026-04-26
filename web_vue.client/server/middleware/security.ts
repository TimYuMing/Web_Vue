/**
 * CSP & 安全 HTTP Headers
 *
 * 策略：
 *   - dev  模式：寬鬆（unsafe-inline + unsafe-eval），讓 Vite HMR / source maps 正常運作
 *   - prod 模式：嚴格 nonce-based CSP，Nitro plugin（server/plugins/nonce.ts）負責
 *                在 render:html 時將 nonce 注入所有 inline <script>/<style>
 */

import { randomBytes } from 'node:crypto'

export default defineEventHandler((event) => {
  // 只處理頁面 HTML 請求，靜態資源與 API 不加 CSP
  const url = event.node.req.url ?? ''
  if (
    url.startsWith('/api/') ||
    url.startsWith('/_nuxt/') ||
    url.startsWith('/__nuxt') ||
    /\.(js|mjs|css|png|jpg|jpeg|gif|webp|svg|ico|woff2?|ttf|eot)(\?.*)?$/.test(url)
  ) {
    return
  }

  const isDev = process.env.NODE_ENV === 'development'

  let csp: string

  if (isDev) {
    // ── 開發模式：寬鬆 CSP ────────────────────────────────────────────────────
    // Vite HMR 需要 unsafe-eval（source maps、dynamic import）
    // Vite 的 style injection 需要 unsafe-inline
    // HMR WebSocket 需要 ws: / wss:
    csp = [
      `default-src 'self'`,
      `script-src 'self' 'unsafe-inline' 'unsafe-eval'`,
      `style-src 'self' 'unsafe-inline'`,
      `img-src 'self' data: blob:`,
      `font-src 'self' data:`,
      `connect-src 'self' ws: wss:`,
      `frame-ancestors 'none'`,
      `form-action 'self'`,
      `base-uri 'self'`,
    ].join('; ')
  } else {
    // ── 生產模式：nonce-based 嚴格 CSP ──────────────────────────────────────
    const nonce = randomBytes(16).toString('base64')
    event.context.cspNonce = nonce

    csp = [
      `default-src 'self'`,
      // nonce 信任 inline script（Nuxt hydration）；strict-dynamic 允許它們再動態載入
      `script-src 'nonce-${nonce}' 'strict-dynamic'`,
      `style-src 'self' 'nonce-${nonce}'`,
      `img-src 'self' data: blob:`,
      `font-src 'self'`,
      `connect-src 'self'`,
      `media-src 'none'`,
      `object-src 'none'`,
      `frame-src 'none'`,
      `frame-ancestors 'none'`,
      `form-action 'self'`,
      `base-uri 'self'`,
      `upgrade-insecure-requests`,
    ].join('; ')
  }

  setResponseHeader(event, 'Content-Security-Policy', csp)
  setResponseHeader(event, 'X-Content-Type-Options', 'nosniff')
  setResponseHeader(event, 'X-Frame-Options', 'DENY')
  setResponseHeader(event, 'Referrer-Policy', 'strict-origin-when-cross-origin')
  setResponseHeader(event, 'X-XSS-Protection', '0')
  setResponseHeader(event, 'Permissions-Policy',
    'camera=(), microphone=(), geolocation=(), payment=(), usb=(), bluetooth=()')

  if (!isDev) {
    setResponseHeader(event, 'Strict-Transport-Security',
      'max-age=63072000; includeSubDomains; preload')
  }

  removeResponseHeader(event, 'X-Powered-By')
  removeResponseHeader(event, 'Server')
})
