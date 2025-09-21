<script setup lang="ts">
import type { CardProps } from '@nuxt/ui'

const { icon, label, ...props } = withDefaults(defineProps<{
  icon?: string
  label: string
} & CardProps>(), {
  variant: 'outline',
})
</script>

<template>
  <UCard v-bind="props">
    <template #header>
      <UiDataMedia
        :icon
        :label
        size="xl"
      />
    </template>
    <slot />

    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </UCard>
</template>
