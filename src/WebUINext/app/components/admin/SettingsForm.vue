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
    :ui="{
      footer: 'flex items-center justify-center gap-4',
    }"
  >
    <template #header>
      <UiDataCell>
        <template #leftContent>
          <UIcon name="crpg:settings" class="size-6" />
        </template>
        <div class="text-md">
          Site Settings
        </div>
      </UiDataCell>
    </template>
    <div class="space-y-6">
      <UFormField
        v-for="(_, key) in settingModel" :key
        :label="capitalize(key)"
      >
        <UInput
          v-model="settingModel[key]"
          type="text"
          color="neutral"
          variant="outline"
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
      <ConfirmActionPopover
        :confirm-label="$t('action.ok')"
        title="Are you sure you want to remove the setting?"
        @confirm="$emit('submit', settingModel)"
      >
        <UButton
          size="lg"
          :loading
          :label="$t('action.save')"
        />
      </ConfirmActionPopover>
    </template>
  </UCard>
</template>
