<script setup lang="ts">
import { capitalize } from 'es-toolkit'

import type { Settings } from '~/models/setting'

const props = defineProps<{
  settings: Settings
  loading: boolean
}>()

defineEmits<{
  submit: [settings: Settings]
  reset: []
}>()

const settingModel = ref<Settings>({ ...props.settings })

watch(() => props.settings, () => {
  settingModel.value = { ...props.settings }
})
</script>

<template>
  <UCard
    label="Settings"
    icon="settings"
    :ui="{
      footer: 'flex items-center justify-center gap-4',
    }"
  >
    <div class="space-y-6">
      <UFormField
        v-for="(_, key) in settingModel" :key
        :label="capitalize(key)"
      >
        <UInput
          v-model="settingModel[key]"
          type="text"
          color="secondary"
          variant="outline"
          size="lg"
          class="w-full"
        />
      </UFormField>
    </div>

    <template #footer>
      <UButton
        variant="outline"
        size="lg"
        :label="$t('action.reset')"
        @click="$emit('reset')"
      />
      <AppConfirmActionTooltip
        :confirm-label="$t('action.ok')"
        title="Are you sure you want to remove the setting?"
        @confirm="$emit('submit', settingModel)"
      >
        <UButton
          size="lg"
          :loading
          :label="$t('action.save')"
        />
      </AppConfirmActionTooltip>
    </template>
  </UCard>
</template>
