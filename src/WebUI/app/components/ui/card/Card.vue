<script setup lang="ts">
import type { CardProps } from '@nuxt/ui'

const { icon, label, variant = 'outline', ...props } = defineProps<{
  icon?: string
  label?: string
} & CardProps>()
</script>

<template>
  <UCard v-bind="{ ...props, variant }">
    <template v-if="label" #header>
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
