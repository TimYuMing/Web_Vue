/**
 * C# → TypeScript 模型同步工具
 *
 * 功能：
 *   1. 解析後端 Web_Vue.Server/Enums/*.cs → 生成 types/enum/*.ts
 *   2. 解析後端 Web_Vue.Server/ViewModels/**‌/*.cs (排除 Base/) → 生成 types/viewmodel/**‌/*.ts
 *
 * Enum 的設計：
 *   每個 enum 值是一個 { Value, Text } 物件；整個 enum 也可以直接當數字用。
 *   例如：
 *     ResultType.Success        → 1  (透過 Symbol.toPrimitive)
 *     ResultType.Success.Value  → 1
 *     ResultType.Success.Text   → "Txt_成功"
 *
 * 執行方式：
 *   node scripts/generate-models.mjs
 *
 * 也可以加入 nuxt.config.ts hooks 在 build 時自動執行。
 */

import fs from 'node:fs'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

/** 讀取 UTF-8 檔案（自動去除 BOM） */
function readUtf8(filePath) {
  let content = fs.readFileSync(filePath, 'utf-8')
  // 去除 BOM (EF BB BF)
  if (content.charCodeAt(0) === 0xFEFF) {
    content = content.slice(1)
  }
  return content
}

const __dirname = path.dirname(fileURLToPath(import.meta.url))
const serverRoot = path.resolve(__dirname, '../../Web_Vue.Server')
const clientTypesRoot = path.resolve(__dirname, '../types')

const enumSrcDir = path.resolve(serverRoot, 'Enums')
const vmSrcDir = path.resolve(serverRoot, 'ViewModels')
const enumOutDir = path.resolve(clientTypesRoot, 'enum')
const vmOutDir = path.resolve(clientTypesRoot, 'viewmodel')

// ─── 工具函式 ────────────────────────────────────────────────────────────────

/** 取得目錄下所有 .cs 檔案（遞迴） */
function collectCsFiles(dir, arr = []) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name)
    if (entry.isDirectory()) {
      collectCsFiles(full, arr)
    } else if (entry.name.endsWith('.cs')) {
      arr.push(full)
    }
  }
  return arr
}

/** C# 型別 → TypeScript 型別對應 */
function mapCsTypeToTs(csType) {
  csType = csType.trim()

  // 去掉 ? nullable
  const nullable = csType.endsWith('?')
  const base = nullable ? csType.slice(0, -1) : csType

  let ts
  switch (base.toLowerCase()) {
    case 'string':    ts = 'string'; break
    case 'int':
    case 'long':
    case 'double':
    case 'float':
    case 'decimal':   ts = 'number'; break
    case 'bool':      ts = 'boolean'; break
    case 'datetime':  ts = 'string'; break  // ISO 8601 string
    case 'object':    ts = 'unknown'; break
    default: {
      // List<T> → T[]
      const listMatch = base.match(/^List<(.+)>$/i)
      if (listMatch) {
        return `${mapCsTypeToTs(listMatch[1])}[]`
      }
      // IEnumerable<T> → T[]
      const ieMatch = base.match(/^IEnumerable<(.+)>$/i)
      if (ieMatch) {
        return `${mapCsTypeToTs(ieMatch[1])}[]`
      }
      // 其他保留 PascalCase（ViewModel、Enum 等）
      ts = base
    }
  }

  return nullable ? `${ts} | null` : ts
}

/** 將 PascalCase ViewModel 名稱轉為 camelCase 屬性名稱 */
function toCamelCase(str) {
  return str.charAt(0).toLowerCase() + str.slice(1)
}

// ─── Enum 解析 & 生成 ────────────────────────────────────────────────────────

/**
 * 解析單一 .cs 檔案，擷取所有 enum 定義
 * 回傳格式：Array<{ name, summary, members: Array<{ name, value, description }> }>
 */
function parseEnums(content) {
  const enums = []

  // 找每個 enum 塊
  const enumRegex = /\/\/\/\s*<summary>\s*(.*?)\s*<\/summary>[\s\S]*?public enum (\w+)\s*\{([\s\S]*?)\}/g
  let match
  while ((match = enumRegex.exec(content)) !== null) {
    const summary = match[1].trim()
    const name = match[2]
    const body = match[3]

    const members = []
    // 逐行解析成員
    const lines = body.split('\n')
    let pendingDescription = ''
    let pendingSummary = ''

    for (const line of lines) {
      const trimmed = line.trim()

      // /// <summary> 描述 </summary>
      const summaryMatch = trimmed.match(/\/\/\/\s*<summary>\s*(.*?)\s*<\/summary>/)
      if (summaryMatch) {
        pendingSummary = summaryMatch[1].trim()
        continue
      }

      // [Description("...")]
      const descMatch = trimmed.match(/\[Description\("([^"]+)"\)\]/)
      if (descMatch) {
        pendingDescription = descMatch[1]
        continue
      }

      // 成員 = 值
      const memberMatch = trimmed.match(/^(\w+)\s*=\s*(\d+)/)
      if (memberMatch) {
        members.push({
          name: memberMatch[1],
          value: parseInt(memberMatch[2], 10),
          description: pendingDescription,
          summary: pendingSummary,
        })
        pendingDescription = ''
        pendingSummary = ''
      }
    }

    if (members.length > 0) {
      enums.push({ name, summary, members })
    }
  }

  return enums
}

/**
 * 將 enum 定義轉為單一 block（不含檔頭註解）
 */
function generateEnumBlock(enumDef) {
  const { name, summary, members } = enumDef

  const memberLines = members.map(m => {
    const comment = m.summary ? `  /** ${m.summary} */\n` : ''
    return (
      `${comment}` +
      `  ${m.name}: Object.freeze({\n` +
      `    Value: ${m.value},\n` +
      `    Text: '${m.description}',\n` +
      `    toString() { return String(${m.value}) },\n` +
      `    [Symbol.toPrimitive](hint: string) { return hint === 'string' ? '${m.value}' : ${m.value} },\n` +
      `  }),`
    )
  }).join('\n')

  const valueType = members.map(m => m.value).join(' | ')
  const textType = members.map(m => `'${m.description}'`).join(' | ')

  return `/** ${summary} */
export interface ${name}Member {
  Value: ${valueType}
  Text: ${textType}
  toString(): string
  [Symbol.toPrimitive](hint: string): number | string
}

/** ${summary} */
export const ${name} = Object.freeze({
${memberLines}
} as const satisfies Record<string, ${name}Member>)

export type ${name}Value = ${valueType}
`
}

function generateEnums() {
  if (!fs.existsSync(enumSrcDir)) {
    console.warn(`[generate-models] Enum 來源目錄不存在：${enumSrcDir}`)
    return
  }

  fs.mkdirSync(enumOutDir, { recursive: true })

  const allEnums = []
  for (const file of collectCsFiles(enumSrcDir)) {
    const content = readUtf8(file)
    const parsed = parseEnums(content)
    allEnums.push(...parsed)
  }

  // 所有 enum 集中輸出到單一 Enums.ts（比照後端 Enums.cs 管理方式）
  const blocks = allEnums.map(e => generateEnumBlock(e)).join('\n')
  const enumsFile = path.join(enumOutDir, 'Enums.ts')
  const fileContent =
    `// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!\n` +
    `// 來源：Web_Vue.Server/Enums\n\n` +
    blocks
  fs.writeFileSync(enumsFile, fileContent, 'utf-8')
  console.log(`[generate-models] Enum → types/enum/Enums.ts`)

  // index.ts re-export Enums.ts
  const index = `// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!\nexport * from './Enums'\n`
  fs.writeFileSync(path.join(enumOutDir, 'index.ts'), index, 'utf-8')
  console.log(`[generate-models] Enum index → types/enum/index.ts`)
}

// ─── ViewModel 解析 & 生成 ───────────────────────────────────────────────────

/** 已知 enum 名稱清單（用來判斷屬性型別是否為 enum） */
let knownEnumNames = new Set()

/**
 * 解析單一 .cs 檔案，擷取所有 class/record 定義（排除 abstract）
 * 回傳格式：Array<{ name, summary, properties: Array<{name, tsType, summary}> }>
 */
function parseViewModels(content) {
  const models = []

  // 找每個 class 塊（不抓 abstract 和 static），並捕捉泛型參數（如 <T>）
  const classRegex =
    /\/\/\/\s*<summary>\s*(.*?)\s*<\/summary>[\s\S]*?public\s+(?!abstract\s|static\s)(?:class|record)\s+(\w+)(<\w+>)?\s*(?:where\s.*?)?(?::[^{]+)?\{([\s\S]*?)\n\}/g

  let match
  while ((match = classRegex.exec(content)) !== null) {
    const summary = match[1].trim()
    const name = match[2]
    // 捕捉 C# 泛型型別參數，例如 <T> → ['T']
    const typeParams = match[3] ? match[3].slice(1, -1).split(',').map(s => s.trim()) : []
    const body = match[4]

    const properties = []
    const lines = body.split('\n')
    let pendingSummary = ''

    for (const line of lines) {
      const trimmed = line.trim()

      const summaryMatch = trimmed.match(/\/\/\/\s*<summary>\s*(.*?)\s*<\/summary>/)
      if (summaryMatch) {
        pendingSummary = summaryMatch[1].trim()
        continue
      }

      // public T Name { get; set; }
      // public T? Name { get; set; }
      // public T Name { get; }   (computed)
      const propMatch = trimmed.match(
        /^public\s+([\w<>?\[\]]+)\s+(\w+)\s*\{/
      )
      if (propMatch) {
        const rawCsType = propMatch[1]
        const propName = propMatch[2]

        // 跳過不需要的
        if (['get', 'set', 'override', 'virtual', 'static'].includes(propName)) {
          pendingSummary = ''
          continue
        }

        properties.push({
          name: propName,
          csType: rawCsType,
          tsType: mapCsTypeToTs(rawCsType),
          summary: pendingSummary,
          isEnum: knownEnumNames.has(rawCsType.replace('?', '')),
        })
        pendingSummary = ''
      } else {
        // 非 property 行，重置
        if (trimmed && !trimmed.startsWith('//') && !trimmed.startsWith('[')) {
          pendingSummary = ''
        }
      }
    }

    if (properties.length > 0) {
      models.push({ name, summary, typeParams, properties })
    }
  }

  return models
}

function generateViewModelTs(model, enumImports) {
  const { name, summary, typeParams = [], properties } = model

  // 自動偵測 C# 泛型：若 class 宣告含有 <T>，直接輸出泛型 TS 介面
  // 例如 ResponseViewModel<T> → export interface ResponseViewModel<T = unknown>
  const genericSuffix = typeParams.length > 0
    ? `<${typeParams.map(t => `${t} = unknown`).join(', ')}>`
    : ''
  const typeParamSet = new Set(typeParams)

  // 收集需要 import 的 enum
  const usedEnums = new Set()
  for (const p of properties) {
    if (p.isEnum) {
      const baseName = p.csType.replace('?', '')
      usedEnums.add(baseName)
    }
  }

  const importLines = []
  if (usedEnums.size > 0) {
    const enumValueTypes = [...usedEnums].map(e => `${e}Value`).join(', ')
    importLines.push(`import type { ${enumValueTypes} } from '../enum'`)
  }

  const propLines = properties.map(p => {
    const comment = p.summary ? `  /** ${p.summary} */\n` : ''
    // enum 屬性用 EnumValue 類型（數字聯合型別）
    let tsType = p.tsType
    if (p.isEnum) {
      const baseName = p.csType.replace('?', '')
      const nullable = p.csType.endsWith('?')
      tsType = nullable ? `${baseName}Value | null` : `${baseName}Value`
    }
    // 泛型屬性：若此屬性的 C# 型別本身就是型別參數（如 T 或 T?），改用 TS 泛型
    const baseType = p.csType.replace('?', '')
    if (typeParamSet.has(baseType)) {
      tsType = p.csType.endsWith('?') ? `${baseType} | null` : baseType
    }
    const optional = tsType.includes('| null') ? '' : ''
    return `${comment}  ${p.name}: ${tsType}`
  }).join('\n')

  const importsBlock = importLines.length > 0
    ? importLines.join('\n') + '\n\n'
    : ''

  return `// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!
// 來源：Web_Vue.Server/ViewModels
${importsBlock}/** ${summary} */
export interface ${name}${genericSuffix} {
${propLines}
}
`
}

function generateViewModels() {
  if (!fs.existsSync(vmSrcDir)) {
    console.warn(`[generate-models] ViewModel 來源目錄不存在：${vmSrcDir}`)
    return
  }

  // 先掃 enum 名稱
  if (fs.existsSync(enumSrcDir)) {
    for (const file of collectCsFiles(enumSrcDir)) {
      const content = readUtf8(file)
      const regex = /public enum (\w+)/g
      let m
      while ((m = regex.exec(content)) !== null) {
        knownEnumNames.add(m[1])
      }
    }
  }

  fs.mkdirSync(vmOutDir, { recursive: true })

  // 收集所有 cs 檔案，排除 Base/ 資料夾
  const allFiles = collectCsFiles(vmSrcDir).filter(f => {
    const rel = path.relative(vmSrcDir, f)
    return !rel.startsWith('Base' + path.sep) && !rel.startsWith('Base/')
  })

  // 按子目錄分組
  const bySubdir = {}
  for (const file of allFiles) {
    const rel = path.relative(vmSrcDir, file)
    const parts = rel.split(path.sep)
    const subdir = parts.length > 1 ? parts[0] : ''
    if (!bySubdir[subdir]) bySubdir[subdir] = []
    bySubdir[subdir].push(file)
  }

  const rootExports = []

  for (const [subdir, files] of Object.entries(bySubdir)) {
    const outSubdir = subdir ? path.join(vmOutDir, subdir) : vmOutDir
    fs.mkdirSync(outSubdir, { recursive: true })

    const subExports = []

    for (const file of files) {
      const content = readUtf8(file)
      const models = parseViewModels(content)

      if (models.length === 0) continue

      // 多個 class 在同個檔案 → 合併到同一個 .ts
      const baseName = path.basename(file, '.cs')
      const outFile = path.join(outSubdir, `${baseName}.ts`)

      const blocks = models.map(m => generateViewModelTs(m, knownEnumNames))
      // 合併 import（去重）
      const importSet = new Set()
      const cleanBlocks = blocks.map(block => {
        const importLines = []
        const rest = []
        for (const line of block.split('\n')) {
          if (line.startsWith('import ')) {
            importLines.push(line)
          } else {
            rest.push(line)
          }
        }
        importLines.forEach(l => importSet.add(l))
        return rest.join('\n')
      })

      const finalContent =
        `// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!\n` +
        `// 來源：Web_Vue.Server/ViewModels\n` +
        (importSet.size > 0 ? [...importSet].join('\n') + '\n\n' : '') +
        cleanBlocks.map(b => b.replace(/^\/\/ !! 此檔案.*\n\/\/ 來源.*\n/, '').trimStart()).join('\n')

      fs.writeFileSync(outFile, finalContent, 'utf-8')
      console.log(`[generate-models] ViewModel → ${path.relative(clientTypesRoot, outFile)}`)

      const relExport = subdir
        ? `./${subdir}/${baseName}`
        : `./${baseName}`
      subExports.push(`export * from './${baseName}'`)
    }

    // 子目錄 index.ts
    if (subdir && subExports.length > 0) {
      const subIndex = `// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!\n` +
        subExports.join('\n') + '\n'
      fs.writeFileSync(path.join(outSubdir, 'index.ts'), subIndex, 'utf-8')
      rootExports.push(`export * from './${subdir}'`)
    } else {
      // root 層
      for (const f of files) {
        const baseName = path.basename(f, '.cs')
        rootExports.push(`export * from './${baseName}'`)
      }
    }
  }

  // root index.ts
  const rootIndex = `// !! 此檔案由 generate-models.mjs 自動生成，請勿手動修改 !!\n` +
    rootExports.join('\n') + '\n'
  fs.writeFileSync(path.join(vmOutDir, 'index.ts'), rootIndex, 'utf-8')
  console.log(`[generate-models] ViewModel index → types/viewmodel/index.ts`)
}

// ─── 主程式 ──────────────────────────────────────────────────────────────────

console.log('[generate-models] 開始轉換 C# → TypeScript...')
generateEnums()
generateViewModels()
console.log('[generate-models] 完成！')
