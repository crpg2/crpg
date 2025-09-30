<script setup lang="ts">
import type { Settings } from '~/models/setting'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ROLE } from '~/models/role'
import { editSettings, getSettings } from '~/services/settings-service'

const { settings } = useAppConfig()

definePageMeta({
  roles: [ROLE.Admin],
})

const [onEditSettings, editingSetting] = useAsyncCallback(
  async (settings: Partial<Settings>) => {
    await editSettings(settings)
    updateAppConfig({ settings: await getSettings() })
  },
)
</script>

<template>
  <UContainer class="space-y-12 py-12">
    <UiTextView variant="h1" tag="h1" class="text-center">
      {{ $t('nav.main.Admin') }}
    </UiTextView>

    <AdminSettingsForm
      class="
        mx-auto
        xl:w-1/2
      "
      :settings
      :loading="editingSetting"
      @submit="onEditSettings"
    />
  </UContainer>
</template>
