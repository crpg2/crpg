<script setup lang="ts">
import type { Settings } from '~/models/setting'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePageLoading } from '~/composables/utils/use-page-loading'
import { ROLE } from '~/models/role'
import { editSettings } from '~/services/settings-service'
import { useSettingsStore } from '~/stores/settings'

const { togglePageLoading } = usePageLoading()

definePageMeta({
  roles: [ROLE.Admin],
})

const settingStore = useSettingsStore()

const {
  execute: onEditSettings,
  isLoading: editingSetting,
} = useAsyncCallback(
  async (settings: Partial<Settings>) => {
    await editSettings(settings)
    await settingStore.loadSettings()
  },
)

watchEffect(() => {
  togglePageLoading(settingStore.isLoadingSettings || editingSetting.value)
})
</script>

<template>
  <UContainer class="py-12">
    <h1 class="mb-14 text-center text-xl text-content-100">
      {{ $t('nav.main.Admin') }}
    </h1>

    <AdminSettingsForm
      class="
        mx-auto
        xl:w-1/2
      "
      :settings="settingStore.settings"
      :loading="editingSetting"
      @reset="settingStore.loadSettings"
      @submit="onEditSettings"
    />
  </UContainer>
</template>
