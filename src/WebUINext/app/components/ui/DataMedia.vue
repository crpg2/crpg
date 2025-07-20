<script setup lang="ts">
import { tv } from 'tailwind-variants'

type Size = 'md' | 'lg'

const { size = 'md', layout } = defineProps<{
  icon?: string
  label?: string
  layout?: 'normal' | 'reverse'
  size?: Size
}>()

const variants = tv({
  slots: {
    root: 'inline-flex items-center align-middle',
    icon: '',
    label: '',
  },
  variants: {
    layout: {
      normal: {
        root: 'flex-row',
      },
      reverse: {
        root: 'flex-row-reverse',
      },
    },
    size: {
      md: {
        root: 'gap-0.5',
        icon: 'size-5',
        label: 'text-sm',
      },
      lg: {
        root: 'gap-1',
        icon: 'size-6',
        label: 'text-md',
      },
    },
  },
})

const classes = computed(() => variants({
  size,
  layout,
}))
</script>

<template>
  <div :class="classes.root()">
    <slot name="icon">
      <UIcon v-if="icon" :name="icon" :class="classes.icon()" />
    </slot>
    <slot>
      <div v-if="label" :class="classes.label()">
        {{ label }}
      </div>
    </slot>
  </div>
</template>
