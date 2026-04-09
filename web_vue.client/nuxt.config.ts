import fs from 'fs'
import path from 'path'
import child_process from 'child_process'
import { env } from 'process'

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
  css: ['~/assets/css/main.css'],

  // 開發伺服器：與原本 Vite 相同的 port 與 HTTPS 憑證設定
  // listhen 的 resolveCert 接受「檔案路徑」，不需要自己 readFileSync
  devServer: {
    port: parseInt(env.DEV_SERVER_PORT || '64910'),
    https: {
      key: keyFilePath,
      cert: certFilePath,
    },
  },

  // 將 API 請求代理到 .NET 後端
  nitro: {
    devProxy: {
      '/weatherforecast': {
        target: 'https://localhost:7146/weatherforecast',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
