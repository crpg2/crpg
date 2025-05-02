<script setup lang="ts">
const { title } = defineProps<{ title: string }>()
</script>

<template>
  <div class="mb-14 flex select-none items-center justify-center gap-4 md:gap-8">
    <SpriteSymbol
      name="logo-decor"
      viewBox="0 0 108 10"
      inline
      class="w-16 rotate-180 md:w-28"
    />
    <h1 class="text-center text-2xl text-content-100">
      {{ title }}
    </h1>
    <SpriteSymbol
      name="logo-decor"
      viewBox="0 0 108 10"
      inline
      class="w-16 md:w-28"
    />
  </div>
</template>
