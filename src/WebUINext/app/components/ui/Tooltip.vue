<script setup lang="ts">
const {
  description,
  disabled = false,
  label,
  title,
} = defineProps<{
  label?: string
  title?: string | null
  description?: string | null
  disabled?: boolean
}>()

const isDisabled = computed(() => disabled || !title)
</script>

<template>
  <VTooltip
    placement="auto"
    :disabled="isDisabled"
  >
    <div>
      <slot>{{ label }}</slot>
    </div>
    <template #popper>
      <div class="prose prose-invert space-y-3">
        <h5>
          {{ title }}
        </h5>
        <div
          v-if="description"
          class="text-2xs"
          v-html="description"
        />
      </div>
    </template>
  </VTooltip>
</template>
