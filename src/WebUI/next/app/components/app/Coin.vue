<script setup lang="ts">
const { value } = defineProps<{ value: number }>()
</script>

<template>
  <span class="inline-flex gap-1.5 align-middle font-bold text-primary">
    <SpriteSymbol name="coin" viewBox="0 0 18 18" class="w-4" inline />
    <slot>
      {{ $n(value) }}
    </slot>
  </span>
</template>
