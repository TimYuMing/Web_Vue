import fs from 'fs'
import path from 'path'
import child_process from 'child_process'
import { env } from 'process'

// =============================================================
// i18n 自動生成工具：讀取後端 resx → 生成前端 i18n JSON
// 統一由後端 Resources/*.resx 管理所有前後端語系
// =============================================================

/** 解碼 XML 實體字元 */
function decodeXmlEntities(str: string): string {
  return str
    .replace(/&amp;/g, '&')
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>')
    .replace(/&quot;/g, '"')
    .replace(/&apos;/g, "'")
}

/** 從 resx XML 內容解析 key-value 對 */
function parseResx(content: string): Record<string, string> {
  const result: Record<string, string> = {}
  const regex = /<data\s+name="([^"]+)"[^>]*>\s*<value>([^<]*)<\/value>/g
  let match
  while ((match = regex.exec(content)) !== null) {
    result[match[1]] = decodeXmlEntities(match[2].trim())
  }
  return result
}

/**
 * 讀取 Web_Vue.Server/Resources/Lang.{locale}.resx
 * 生成 web_vue.client/i18n/locales/{locale}.json
 */
function generateI18nLocales(): void {
  const resxDir = path.resolve(process.cwd(), '../Web_Vue.Server/Resources')
  const outputDir = path.resolve(process.cwd(), 'i18n/locales')

  if (!fs.existsSync(resxDir)) {
    console.warn('[i18n-gen] Resources 目錄不存在，跳過生成')
    return
  }
  if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true })
  }

  const resxFiles = fs.readdirSync(resxDir).filter((f) => /^SharedResource\..+\.resx$/.test(f))
  for (const file of resxFiles) {
    const m = file.match(/^SharedResource\.(.+)\.resx$/)
    if (!m) {
      continue
    }
    const locale = m[1]
    const content = fs.readFileSync(path.join(resxDir, file), 'utf-8')
    const translations = parseResx(content)
    const outputPath = path.join(outputDir, `${locale}.json`)
    fs.writeFileSync(outputPath, JSON.stringify(translations, null, 2) + '\n', 'utf-8')
    console.log(`[i18n-gen] ${locale}.json → ${Object.keys(translations).length} keys`)
  }
}

// 在 nuxt.config.ts 載入時立即執行，確保 i18n 模組初始化前 JSON 已存在
generateI18nLocales()

// =============================================================
// generate-models：解析後端 C# Enum / ViewModel → 生成前端 TypeScript
// 讓前後端 Model 保持同步，在 dev / build 時自動執行
// =============================================================
function generateModels(): void {
  const scriptPath = path.resolve(process.cwd(), 'scripts/generate-models.mjs')
  if (!fs.existsSync(scriptPath)) {
    console.warn('[generate-models] 腳本不存在，跳過')
    return
  }
  const result = child_process.spawnSync(process.execPath, [scriptPath], {
    stdio: 'inherit',
    env: process.env,
  })
  if (result.status !== 0) {
    console.error('[generate-models] 執行失敗')
  }
}

generateModels()

// =============================================================

// 與原本 vite.config.js 相同的邏輯：讀取 dotnet dev-certs 產生的憑證
const baseFolder =
  env.APPDATA !== undefined && env.APPDATA !== ''
    ? `${env.APPDATA}/ASP.NET/https`
    : `${env.HOME}/.aspnet/https`

const certificateName = 'web_vue.client'
const certFilePath = path.join(baseFolder, `${certificateName}.pem`)
const keyFilePath = path.join(baseFolder, `${certificateName}.key`)

if (!fs.existsSync(baseFolder)) {
  fs.mkdirSync(baseFolder, { recursive: true })
}

if (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath)) {
  if (
    child_process.spawnSync(
      'dotnet',
      ['dev-certs', 'https', '--export-path', certFilePath, '--format', 'Pem', '--no-password'],
      { stdio: 'inherit' },
    ).status !== 0
  ) {
    throw new Error('Could not create certificate.')
  }
}

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-01-01',

  devtools: { enabled: true },

  // SSR 預設啟用，提供 SEO 最佳化的伺服器端渲染
  ssr: true,

  // 全域 CSS
  css: ['~/assets/css/share/main.css'],

  // 載入的 Nuxt 模組
  modules: ['@nuxtjs/i18n'],

  // i18n 設定：所有語系由後端 resx 統一管理，build 時自動生成 JSON
  i18n: {
    locales: [
      { code: 'zh-TW', language: 'zh-TW', file: 'zh-TW.json', name: '繁體中文' },
      { code: 'en-US', language: 'en-US', file: 'en-US.json', name: 'English' },
    ],
    defaultLocale: 'zh-TW',
    lazy: true,
    langDir: 'locales',
    strategy: 'no_prefix',
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'i18n_locale',
      alwaysRedirect: false,
      fallbackLocale: 'zh-TW',
    },
  },

  // 開發伺服器：與原本 Vite 相同的 port 與 HTTPS 憑證設定
  // listhen 的 resolveCert 接受「檔案路徑」，不需要自己 readFileSync
  devServer: {
    port: parseInt(env.DEV_SERVER_PORT || '64910'),
    https: {
      key: keyFilePath,
      cert: certFilePath,
    },
  },

  // Vite 插件：開發模式下監聽 resx 變更，自動重新生成 i18n JSON
  vite: {
    plugins: [
      {
        name: 'resx-watcher',
        configureServer(server: any) {
          const resxDir = path.resolve(process.cwd(), '../Web_Vue.Server/Resources')
          server.watcher.add(resxDir)
          server.watcher.on('change', (filePath: string) => {
            if (filePath.endsWith('.resx')) {
              console.log('[i18n-gen] resx 異動，重新生成語系 JSON...')
              generateI18nLocales()
            }
          })
        },
      },
    ],
  },

  // 將 /api/** 請求代理到 .NET 後端
  routeRules: {
    '/api/**': {
      proxy: 'http://localhost:45336/api/**',
    },
  },
})
