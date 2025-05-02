<script setup lang="ts">
const { inline = false } = defineProps<{ inline?: boolean }>()
</script>

<template>
  <div v-if="inline" class="h-6 w-px select-none bg-border-200" />
  <SpriteSymbol
    v-else name="divider"
    viewBox="0 0 480 19"
    inline
    class="w-full fill-current"
  />
</template>
