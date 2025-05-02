<script setup lang="ts">
const { disabled = false } = defineProps<{
  title?: string
  confirmLabel?: string
  disabled?: boolean
}>()

defineEmits<{
  cancel: []
  confirm: []
}>()
</script>

<template>
  <VTooltip :triggers="['click']" :disabled>
    <slot />
    <template #popper="{ hide }">
      <div class="space-y-3">
        <div>
          {{ title !== undefined ? title : $t('confirmAction') }}
        </div>

        <div class="flex items-center gap-2">
          <OButton
            variant="success"
            size="2xs"
            icon-left="check"
            :label="confirmLabel !== undefined ? confirmLabel : $t('action.confirm')"
            @click="
              () => {
                $emit('confirm');
                hide();
              }
            "
          />
          <OButton
            variant="danger"
            size="2xs"
            icon-left="close"
            :label="$t('action.cancel')"
            @click="
              () => {
                $emit('cancel');
                hide();
              }
            "
          />
        </div>
      </div>
    </template>
  </VTooltip>
</template>
