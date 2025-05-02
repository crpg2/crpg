<script setup lang="ts">
const { point } = defineProps<{ point: number }>()
</script>

<template>
  <div
    v-tooltip.bottom="$t('user.field.heirloom')"
    class="inline-flex items-center gap-1.5 align-middle font-bold text-primary"
  >
    <OIcon icon="blacksmith" size="lg" />
    <slot>
      {{ $n(point) }}
    </slot>
  </div>
</template>
