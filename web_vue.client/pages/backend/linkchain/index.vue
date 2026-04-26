<script setup lang="ts">
  import { ResultType } from '~/types/enum'

  definePageMeta({
    layout: 'backend',
    title: '凌誠功能頁',
  })

  useHead({ title: '凌誠 | 管理後台' })

  const $resx = useResx()
  const api = useApi()
  const { alertKit } = useAlert()
  const { permissions } = useAuth()

  // 權限代碼（對應 PermissionCodes.Backend.Linkchain）
  const PERM = {
    Index: '52813caf-f8e5-4632-9e32-6d8c55741e59',
    PermissionScanning: 'df63d067-c070-49a5-a6f1-ec4dbc311515',
  }

  const hasPermission = (code: string) => permissions.value.includes(code)

  const loadingKey = ref<string | null>(null)

  const runPermissionScan = async () => {
    loadingKey.value = 'permission-scan'

    try {
      const res = await api.post('/api/linkchain/permission-scan')
      const isSuccess = res.Status === ResultType.Success.Value
      await alertKit({
        title: res.Message || $resx(isSuccess ? 'Txt_操作成功' : 'Txt_操作失敗'),
        icon: isSuccess ? 'success' : 'error',
      })
    } catch {
      await alertKit({ title: $resx('Txt_伺服器錯誤'), icon: 'error' })
    } finally {
      loadingKey.value = null
    }
  }
</script>

<template>
  <div>
    <h1 class="backend-page-heading">{{ $resx('Txt_凌誠') }}</h1>

    <div class="backend-card" style="max-width: 480px">
      <h2 style="font-size: 1rem; font-weight: 600; margin-bottom: 1rem">
        {{ $resx('Txt_功能頁') }}
      </h2>

      <ul style="list-style: none; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 0.75rem">
        <!-- 權限掃描 -->
        <!--<li v-if="hasPermission(PERM.PermissionScanning)">
          <button
            class="btn btn-link p-0 text-start"
            type="button"
            :disabled="loadingKey === 'permission-scan'"
            @click="runPermissionScan"
          >
            {{ loadingKey === 'permission-scan' ? '掃描中...' : $resx('Txt_權限掃描') }}
          </button>
        </li>-->
        <li>
          <button class="btn btn-link p-0 text-start"
                  type="button"
                  :disabled="loadingKey === 'permission-scan'"
                  @click="runPermissionScan">
            {{ loadingKey === 'permission-scan' ? '掃描中...' : $resx('Txt_權限掃描') }}
          </button>
        </li>
      </ul>
    </div>
  </div>
</template>
