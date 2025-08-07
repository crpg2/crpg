<script lang="ts">
export type DataMediaSize = 'xs' | 'sm' | 'md' | 'lg' | 'xl'
</script>

<script setup lang="ts">
// eslint-disable-next-line import/first
import { tv } from 'tailwind-variants'

const { size = 'md', layout } = defineProps<{
  icon?: string
  label?: string
  layout?: 'normal' | 'reverse'
  size?: DataMediaSize
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
      xs: {
        root: 'gap-0.5',
        icon: 'size-3',
        label: 'text-2xs',
      },
      sm: {
        root: 'gap-1',
        icon: 'size-4',
        label: 'text-xs',
      },
      md: {
        root: 'gap-1',
        icon: 'size-5',
        label: 'text-sm',
      },
      lg: {
        root: 'gap-1',
        icon: 'size-6',
        label: 'text-md',
      },
      xl: {
        root: 'gap-1',
        icon: 'size-7',
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
    <slot name="icon" v-bind="{ classes: classes.icon }">
      <UIcon
        v-if="icon"
        :name="icon"
        :class="classes.icon()"
      />
    </slot>

    <slot v-bind="{ classes: classes.label }">
      <div
        v-if="label"
        :class="classes.label()"
      >
        {{ label }}
      </div>
    </slot>
  </div>
</template>
