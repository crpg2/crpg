<script setup lang="ts">
import { capitalize, isEqual } from 'es-toolkit'

import type { Settings } from '~/models/setting'

const props = defineProps<{
  settings: Settings
  loading: boolean
}>()

defineEmits<{
  submit: [settings: Settings]
}>()

const settingModel = ref<Settings>({ ...props.settings })

const isDirty = computed(() => !isEqual(settingModel.value, props.settings))

const reset = () => {
  settingModel.value = { ...props.settings }
}
</script>

<template>
  <UiCard
    :ui="{
      footer: 'flex items-center justify-center gap-4',
    }"
    label="Site Settings"
    icon="crpg:settings"
  >
    <div class="space-y-6">
      <UFormField
        v-for="(_, key) in settingModel" :key
        :label="capitalize(key)"
      >
        <UInput
          v-model="settingModel[key]"
          type="text"
          size="xl"
          color="neutral"
          variant="outline"
          class="w-full"
        />
      </UFormField>
    </div>

    <template #footer>
      <UButton
        variant="outline"
        size="xl"
        :disabled="!isDirty"
        :label="$t('action.reset')"
        @click="reset"
      />
      <AppConfirmActionPopover
        :confirm-label="$t('action.ok')"
        title="Are you sure you want to remove the setting?"
        @confirm="$emit('submit', settingModel)"
      >
        <UButton
          size="xl"
          :disabled="!isDirty"
          :loading
          :label="$t('action.save')"
        />
      </AppConfirmActionPopover>
    </template>
  </UiCard>
</template>
