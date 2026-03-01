<script setup lang="ts">
import { tv } from 'tailwind-variants'

type Size = 'sm' | 'md' | 'lg'

const { layout = 'normal', ellipsis = false, size = 'md' } = defineProps<{
  label?: string
  caption?: string
  layout?: 'normal' | 'reverse'
  ellipsis?: boolean
  size?: Size
}>()

const variants = tv({
  slots: {
    root: 'flex',
    label: 'font-bold text-highlighted',
    caption: 'leading-none text-muted',
  },
  variants: {
    size: {
      sm: {
        root: 'gap-0.5',
        label: 'text-sm',
        caption: 'text-xs',
      },
      md: {
        root: 'gap-1',
        label: 'text-base',
        caption: 'text-sm',
      },
      lg: {
        root: 'gap-1',
        label: 'text-lg',
        caption: 'text-base',
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
    <slot v-bind="{ classes: classes.label }">
      <div
        v-if="label"
        :class="classes.label()"
      >
        {{ label }}
      </div>
    </slot>

    <slot name="caption" v-bind="{ classes: classes.caption }">
      <div
        v-if="caption"
        :class="classes.caption()"
      >
        {{ caption }}
      </div>
    </slot>
  </div>
</template>
