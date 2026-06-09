<script setup lang="ts">
import type { CardProps } from '@nuxt/ui'

const { icon, label, variant = 'outline', ...props } = defineProps<{
  icon?: string
  label?: string
} & CardProps>()
</script>

<template>
  <UCard v-bind="{ ...props, variant }">
    <template #header>
      <slot name="title">
        <UiDataMedia
          v-if="label"
          :icon
          :label
          size="xl"
        />
      </slot>
    </template>

    <template v-if="$slots.default" #default>
      <slot />
    </template>

    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </UCard>
</template>
