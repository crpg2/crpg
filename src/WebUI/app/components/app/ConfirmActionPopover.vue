<script setup lang="ts">
defineProps<{
  title?: string
  confirmLabel?: string
}>()

defineEmits<{
  cancel: []
  confirm: []
}>()

const [open, toggle] = useToggle()
</script>

<template>
  <UPopover
    v-model:open="open"
    :ui="{ content: 'w-64 space-y-3' }"
  >
    <slot />

    <template #content>
      <UiTextView variant="h5">
        {{ title ?? $t('confirmAction') }}
      </UiTextView>

      <div class="flex items-center gap-2">
        <UButton
          variant="soft"
          icon="crpg:close"
          :label="$t('action.cancel')"
          @click="() => {
            $emit('cancel')
            toggle(false)
          }"
        />

        <UButton
          icon="crpg:check"
          :label="confirmLabel ?? $t('action.confirm')"
          @click="() => {
            $emit('confirm')
            toggle(false)
          }"
        />
      </div>
    </template>
  </UPopover>
</template>
