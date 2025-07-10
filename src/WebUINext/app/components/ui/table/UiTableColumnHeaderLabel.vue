<script setup lang="ts">
const { withFilter = false } = defineProps<{
  label: string
  withFilter?: boolean
}>()
</script>

<template>
  <div
    class="text-xs text-default"
    :class="{ 'cursor-pointer underline decoration-dashed underline-offset-6 select-none hover:no-underline': withFilter }"
  >
    {{ label }}
  </div>
</template>
