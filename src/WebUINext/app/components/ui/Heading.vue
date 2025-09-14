<script setup lang="ts">
defineProps<{ title?: string }>()
</script>

<template>
  <div
    class="
      flex items-center justify-center gap-4
      md:gap-8
    "
  >
    <UiSpriteSymbol
      name="logo-decor"
      viewBox="0 0 108 10"
      class="
        w-16 rotate-180
        md:w-28
      "
    />

    <slot>
      <UiTextView v-if="title" tag="h1" variant="h1" class="text-center">
        {{ title }}
      </UiTextView>
    </slot>

    <UiSpriteSymbol
      name="logo-decor"
      viewBox="0 0 108 10"
      class="
        w-16
        md:w-28
      "
    />
  </div>
</template>
