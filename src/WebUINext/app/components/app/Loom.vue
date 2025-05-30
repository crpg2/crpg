<script setup lang="ts">
const { point } = defineProps<{ point: number }>()
</script>

<template>
  <UTooltip :text="$t('user.field.heirloom')">
    <div
      class="inline-flex items-center gap-1.5 align-middle font-bold text-primary"
    >
      <OIcon icon="blacksmith" size="lg" />
      <slot>
        {{ $n(point) }}
      </slot>
    </div>
  </UTooltip>
</template>
