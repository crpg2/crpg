<script setup lang="ts">
const { icon, label } = defineProps<{ icon: string, label: string }>()
</script>

<template>
  <div class="flex items-center gap-1.5">
    <OIcon :icon="icon" size="lg" />
    {{ label }}
  </div>
</template>
