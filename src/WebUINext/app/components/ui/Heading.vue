<script setup lang="ts">
defineProps<{ title?: string }>()
</script>

<template>
  <div class="flex items-center justify-center gap-4 md:gap-8">
    <UiSpriteSymbol
      name="logo-decor"
      viewBox="0 0 108 10"
      class="w-16 rotate-180 md:w-28"
    />
    <slot>
      <h1 v-if="title" class="text-center text-2xl text-highlighted">
        {{ title }}
      </h1>
    </slot>
    <UiSpriteSymbol
      name="logo-decor"
      viewBox="0 0 108 10"
      class="w-16 md:w-28 "
    />
  </div>
</template>
