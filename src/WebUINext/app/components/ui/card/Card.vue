<script setup lang="ts">
defineProps<{
  icon?: string
  label: string
}>()
</script>

<template>
  <UCard variant="outline">
    <template #header>
      <UiDataMedia
        :icon
        :label
        size="xl"
      />
    </template>
    <slot />
  </UCard>
</template>
