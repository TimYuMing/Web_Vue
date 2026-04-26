/**
 * Nonce 注入 Plugin（Nitro server plugin）
 *
 * 在 Nuxt SSR 渲染出 HTML 後，把 cspNonce 注入所有 inline <script> / <style>。
 * 這樣 Nuxt hydration payload 的 inline script 才能通過 CSP nonce 驗證。
 *
 * 依賴：server/middleware/security.ts 已在 event.context.cspNonce 存入 nonce。
 */
export default defineNitroPlugin((nitroApp) => {
  nitroApp.hooks.hook('render:html', (html, { event }) => {
    const nonce = (event.context as any).cspNonce as string | undefined
    if (!nonce) return

    const injectNonce = (chunk: string) =>
      chunk
        .replace(/<script(?![^>]*\bnonce=)/g, `<script nonce="${nonce}"`)
        .replace(/<style(?![^>]*\bnonce=)/g, `<style nonce="${nonce}"`)

    html.head        = html.head.map(injectNonce)
    html.bodyPrepend = html.bodyPrepend.map(injectNonce)
    html.body        = html.body.map(injectNonce)
    html.bodyAppend  = html.bodyAppend.map(injectNonce)
  })
})
