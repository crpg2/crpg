<script setup lang="ts">
import { tv } from 'tailwind-variants'

type Size = 'md' | 'lg'

const { layout = 'normal', ellipsis = false, size = 'md' } = defineProps<{
  label: string
  caption?: string
  layout?: 'normal' | 'reverse'
  ellipsis?: boolean
  size?: Size
}>()

const variants = tv({
  slots: {
    root: 'flex',
    label: 'font-bold text-highlighted',
    caption: 'leading-none',
  },
  variants: {
    size: {
      md: {
        root: '',
        label: 'text-sm',
        caption: 'text-2xs',
      },
      lg: {
        root: '',
        label: 'text-md',
        caption: 'text-xs',
      },
    },
    layout: {
      normal: {
        root: 'flex-col',
      },
      reverse: {
        root: 'flex-col-reverse',
      },
    },
    ellipsis: {
      true: {
        label: 'truncate',
        caption: 'truncate',
      },
      false: {
        root: '',
      },
    },
  },
})

const classes = computed(() => variants({
  layout,
  ellipsis,
  size,
}))
</script>

<template>
  <div :class="classes.root()">
    <div :class="classes.label()">
      {{ label }}
    </div>
    <div v-if="caption" :class="classes.caption()">
      {{ caption }}
    </div>
  </div>
</template>
