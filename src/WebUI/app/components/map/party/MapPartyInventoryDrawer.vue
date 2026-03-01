<script setup lang="ts">
import { usePartyItems } from '~/composables/strategus/use-party'

const emit = defineEmits<{
  close: [value: boolean]
}>()

const { partyItems } = usePartyItems(true)

const onCancel = () => {
  emit('close', false)
}
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    handle-only
    :dismissible="false"
    :ui="{
      header: 'flex items-center justify-center gap-4',
      container: 'w-full max-w-3xl mx-auto',
      footer: 'flex flex-row justify-end',
    }"
  >
    <template #header>
      <div class="flex flex-1 items-center justify-center gap-4">
        <UiTextView variant="h2">
          Inventory
        </UiTextView>
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <ItemStackGrid
        :items="partyItems"
      />
    </template>
  </UDrawer>
</template>
