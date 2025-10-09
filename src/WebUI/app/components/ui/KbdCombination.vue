<script setup lang="ts">
defineProps<{
  label?: string
  keys: string[]
}>()
</script>

<template>
  <div class="flex items-center gap-2">
    <div
      v-for="(kbd, index) in keys"
      :key="index"
      class="flex items-center gap-2"
    >
      <UKbd :value="kbd" />

      <UiTextView
        v-if="index + 1 !== keys.length"
        variant="caption-xs"
      >
        +
      </UiTextView>
    </div>

    <UiTextView v-if="label" variant="caption-xs">
      {{ label }}
    </UiTextView>
  </div>
</template>
