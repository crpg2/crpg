<script setup lang="ts">
defineProps<{
  id: string
  current: number
  max: number
}>()
</script>

<template>
  <div
    :id
    class="text-xs text-muted tabular-nums"
    aria-live="polite"
    role="status"
  >
    {{ current }}/{{ max }}
  </div>
</template>
