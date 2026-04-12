/**
 * i18n 語系生成工具
 * 讀取後端 Web_Vue.Server/Resources/Lang.{locale}.resx
 * 生成前端 web_vue.client/i18n/locales/{locale}.json
 *
 * 執行方式：node scripts/generate-i18n.mjs
 */

import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const resxDir = path.resolve(__dirname, '../../Web_Vue.Server/Resources')
const outputDir = path.resolve(__dirname, '../i18n/locales')

/** 解碼 XML 實體字元 */
function decodeXmlEntities(str) {
  return str
    .replace(/&amp;/g, '&')
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>')
    .replace(/&quot;/g, '"')
    .replace(/&apos;/g, "'")
}

/** 從 resx 內容解析 key-value */
function parseResx(content) {
  const result = {}
  const regex = new RegExp('<data\\s+name="([^"]+)"[^>]*>\\s*<value>([^<]*)<\\/value>', 'g')
  let match
  while ((match = regex.exec(content)) !== null) {
    result[match[1]] = decodeXmlEntities(match[2].trim())
  }
  return result
}

if (!fs.existsSync(resxDir)) {
  console.warn('[i18n-gen] Resources 目錄不存在，跳過生成')
  process.exit(0)
}

if (!fs.existsSync(outputDir)) {
  fs.mkdirSync(outputDir, { recursive: true })
}

const resxFiles = fs.readdirSync(resxDir).filter((f) => /^SharedResource\..+\.resx$/.test(f))

for (const file of resxFiles) {
  const m = file.match(/^SharedResource\.(.+)\.resx$/)
  if (!m) continue
  const locale = m[1]
  const content = fs.readFileSync(path.join(resxDir, file), 'utf-8')
  const translations = parseResx(content)
  const outputPath = path.join(outputDir, `${locale}.json`)
  fs.writeFileSync(outputPath, JSON.stringify(translations, null, 2) + '\n', 'utf-8')
  console.log(`[i18n-gen] ${locale}.json → ${Object.keys(translations).length} keys`)
}
