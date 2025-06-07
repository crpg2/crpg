<script setup lang="ts">
import type { Settings } from '~/models/setting'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { Role } from '~/models/role'
import { editSettings } from '~/services/settings-service'
import { useSettingsStore } from '~/stores/settings'

definePageMeta({
  roles: [Role.Admin],
})

const settingStore = useSettingsStore()

const {
  execute: onEditSettings,
  loading: editingSetting,
} = useAsyncCallback(
  async (settings: Partial<Settings>) => {
    await editSettings(settings)
    await settingStore.loadSettings()
  },
)
</script>

<template>
  <UContainer class="py-12">
    <h1 class="mb-14 text-center text-xl text-content-100">
      {{ $t('nav.main.Admin') }}
    </h1>

    <!-- TODO: -->
    <OLoading
      :active="settingStore.isLoadingSettings"
      icon-size="xl"
    />

    <AdminSettingsForm
      class="mx-auto xl:w-1/2"
      :settings="settingStore.settings"
      :loading="editingSetting"
      @reset="settingStore.loadSettings"
      @submit="onEditSettings"
    />
  </UContainer>
</template>
