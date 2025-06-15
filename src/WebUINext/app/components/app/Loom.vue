<script setup lang="ts">
import { tv } from 'tailwind-variants'

const { size = 'md' } = defineProps<{
  point?: number
  size?: 'md' | 'lg' | 'xl'
}>()

const variants = tv({
  slots: {
    icon: 'text-primary',
    label: 'font-bold',
  },
  variants: {
    size: {
      md: {
        icon: 'size-4',
        label: '',
      },
      lg: {
        icon: 'size-6',
        label: '',
      },
      xl: {
        icon: 'size-8',
        label: 'text-lg',
      },
    },
  },
})
const classes = computed(() => variants({ size }))
</script>

<template>
  <UTooltip :text="$t('user.field.heirloom')">
    <UiDataCell>
      <template #leftContent>
        <UIcon name="crpg:blacksmith" :class="classes.icon()" />
      </template>
      <slot>
        <span v-if="point !== undefined" :class="classes.label()">
          {{ $n(point) }}
        </span>
      </slot>
    </UiDataCell>
  </UTooltip>
</template>
