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
      <span
        v-if="index + 1 !== keys.length"
        class="text-2xs"
      >+</span>
    </div>
    <span v-if="label" class="text-2xs">{{ label }}</span>
  </div>
</template>
