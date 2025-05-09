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
  <UPopover v-model:open="open" :ui="{ content: 'w-64' }">
    <slot />
    <template #content>
      <div class="space-y-3">
        <div>{{ title ?? $t('confirmAction') }}</div>
        <div class="flex items-center gap-2">
          <UButton
            size="xs"
            icon="crpg:check"
            :label="confirmLabel ?? $t('action.confirm')"
            @click="() => {
              $emit('confirm')
              toggle(false)
            }"
          />
          <UButton
            variant="soft"
            size="xs"
            icon="crpg:close"
            :label="$t('action.cancel')"
            @click="() => {
              $emit('cancel')
              toggle(false)
            }"
          />
        </div>
      </div>
    </template>
  </UPopover>
</template>
