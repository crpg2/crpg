<script setup lang="ts">
const { inline = false } = defineProps<{ inline?: boolean }>()
</script>

<!-- TODO: FIXME: rename to decorated divider? -->
<template>
  <div v-if="inline" class="h-6 w-px bg-border-200 select-none" />
  <UiSpriteSymbol
    v-else
    name="divider"
    viewBox="0 0 480 19"
  />
</template>
