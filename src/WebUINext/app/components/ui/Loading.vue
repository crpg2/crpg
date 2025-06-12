<script setup lang="ts">
import { tv } from 'tailwind-variants'

const props = withDefaults(defineProps<{
  active: boolean
  fullPage?: boolean
  size?: 'md'
}>(), {
  fullPage: false,
  size: 'md',
})

const variants = tv({
  slots: {
    root: 'inset-0 flex items-center justify-center overflow-hidden',
    overlay: 'absolute inset-0 bg-base-100/50',
    icon: 'animate-spin',
  },
  variants: {
    size: {
      md: {
        icon: 'size-8',
      },
    },
    fullPage: {
      true: {
        root: 'fixed z-50',
      },
      false: {
        root: 'absolute z-30',
      },
    },
  },
})

const classes = computed(() => variants({
  size: props.size,
  fullPage: props.fullPage,
}))
</script>

<template>
  <Transition name="fade">
    <div
      v-if="active"
      :class="[classes.root()]" v-bind="{ ...(fullPage && { role: 'dialog' }) }"
    >
      <div :class="[classes.overlay()]" :tabindex="-1" />
      <UIcon name="crpg:loading" :class="[classes.icon()]" />
    </div>
  </Transition>
</template>
