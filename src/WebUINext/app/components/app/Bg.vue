<script setup lang="ts">
defineProps<{ bg: string }>()
</script>

<template>
  <div class="fixed inset-0">
    <NuxtImg
      class="size-full object-cover opacity-15"
      :src="`/images/bg/${bg}`"
    />
    <div class="absolute inset-0 bg-gradient-to-t from-default opacity-75" />
  </div>
</template>
