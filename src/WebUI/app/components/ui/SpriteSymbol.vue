<script setup lang="ts">
defineProps<{
  viewBox?: string
  name: string
}>()
</script>

<template>
  <svg aria-hidden="true" :viewBox>
    <use :href="`#${name}`" />
  </svg>
</template>
