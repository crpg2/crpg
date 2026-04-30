<script setup lang="ts">
import type { PopoverProps } from '@nuxt/ui'

defineProps<{
  title?: string
  confirmLabel?: string
  confirmDisabled?: boolean
  content?: PopoverProps['content']
  ui?: PopoverProps['ui']
}>()

defineEmits<{
  cancel: []
  confirm: []
}>()

defineSlots<{
  'default': () => any
  'description-content': () => any
}>()

const [open, toggle] = useToggle()
</script>

<template>
  <UPopover
    v-model:open="open"
    :content
    :ui="{
      content: 'space-y-3 p-0 ring-0 max-w-sm',
      ...ui,
    }"
  >
    <slot />

    <template #content>
      <UiCard
        :ui="{
          footer: 'flex justify-center items-center gap-2',
        }"
        :label="title ?? $t('confirmAction')"
      >
        <template v-if="$slots['description-content']" #default>
          <slot name="description-content" />
        </template>

        <template #footer>
          <UButton
            variant="subtle"
            icon="crpg:close"
            color="neutral"
            block
            :label="$t('action.cancel')"
            @click="() => {
              $emit('cancel')
              toggle(false)
            }"
          />

          <UButton
            icon="crpg:check"
            variant="subtle"
            :label="confirmLabel ?? $t('action.ok')"
            :disabled="confirmDisabled"
            block
            @click="() => {
              $emit('confirm')
              toggle(false)
            }"
          />
        </template>
      </UiCard>
    </template>
  </UPopover>
</template>
