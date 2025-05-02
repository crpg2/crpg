<script setup lang="ts">
import { capitalize } from 'es-toolkit'

import type { Settings } from '~/models/setting'

const props = defineProps<{
  settings: Settings
  loading: boolean
}>()

const emit = defineEmits<{
  submit: [settings: Settings]
  reset: []
}>()

const settingModel = ref<Settings>({ ...props.settings })
watch(() => props.settings, () => {
  settingModel.value = { ...props.settings }
})
</script>

<template>
  <UiFormGroup
    label="Settings"
    icon="settings"
    class="relative mx-auto w-1/2"
  >
    <div class="mb-8 space-y-8">
      <OField v-for="(_, key) in settingModel" :key="key" :label="capitalize(key)">
        <OInput
          v-model="settingModel[key]"
          :placeholder="key"
          type="text"
          expanded
          size="lg"
        />
      </OField>
    </div>

    <div class="sticky bottom-0 flex items-center justify-center gap-4 bg-bg-main/50 py-4 backdrop-blur-sm">
      <OButton
        variant="primary"
        size="lg"
        outlined
        :label="$t('action.reset')"
        @click="emit('reset')"
      />
      <AppConfirmActionTooltip
        class="inline"
        :confirm-label="$t('action.ok')"
        title="Are you sure you want to remove the setting?"
        @confirm="emit('submit', settingModel)"
      >
        <OButton
          variant="primary"
          size="lg"
          :loading="loading"
          :label="$t('action.save')"
        />
      </AppConfirmActionTooltip>
    </div>
  </UiFormGroup>
</template>
