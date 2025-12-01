<script setup lang="ts">
const { withFilter = false } = defineProps<{
  label: string
  description?: string
  withFilter?: boolean
}>()
</script>

<template>
  <UTooltip :disabled="!description">
    <div
      class="text-sm text-default"
      :class="{ 'cursor-pointer underline decoration-dashed underline-offset-6 select-none hover:no-underline': withFilter }"
    >
      {{ label }}
    </div>

    <template #content>
      <UiTooltipContent :title="label" :description />
    </template>
  </UTooltip>
</template>
