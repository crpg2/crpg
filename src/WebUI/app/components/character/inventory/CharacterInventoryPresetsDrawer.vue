<script setup lang="ts">
import type { UserItemPreset } from '~/models/user'

import { useUserItemPresetActions, useUserItemPresets } from '~/composables/user/use-user-item-presets'

const emit = defineEmits<{
  close: [value: boolean]
  apply: [preset: UserItemPreset]
  delete: [presetId: number]
}>()

const onCancel = () => {
  emit('close', false)
}

const { userItemPresets } = useUserItemPresets()
const { onDeleteUserItemPreset } = useUserItemPresetActions()
</script>

<template>
  <UDrawer
    direction="right"
    :handle="false"
    :ui="{
      header: 'flex items-center justify-center gap-4',
      container: 'w-lg flex flex-col overflow-hidden',
      body: 'flex-1 overflow-hidden',
      footer: 'flex flex-row justify-end',
    }"
  >
    <template #header>
      <UiTextView variant="h2">
        {{ $t('character.inventory.presets.title') }}
      </UiTextView>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <div v-if="userItemPresets.length" class="h-full space-y-4 overflow-auto p-2">
        <CharacterInventoryPresetCard
          v-for="preset in userItemPresets"
          :key="preset.id"
          :preset
          @apply="$emit('apply', preset)"
          @delete="onDeleteUserItemPreset(preset.id)"
        />
      </div>
      <UCard v-else class="m-2">
        <UiResultNotFound />
      </UCard>
    </template>
  </UDrawer>
</template>
