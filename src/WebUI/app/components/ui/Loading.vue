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
    root: 'flex items-center justify-center overflow-hidden',
    overlay: 'absolute inset-0 bg-elevated/25',
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
        root: 'pointer-events-auto fixed inset-0 z-[50]',
      },
      false: {
        root: 'absolute inset-0 z-30',
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
  <Transition
    enter-active-class="transition-opacity duration-300 ease-out"
    enter-from-class="opacity-0"
    enter-to-class="opacity-100"
    leave-active-class="transition-opacity duration-200 ease-in"
    leave-from-class="opacity-100"
    leave-to-class="opacity-0"
  >
    <div
      v-if="active"
      :class="[classes.root()]"
      v-bind="{ ...(fullPage && { role: 'dialog' }) }"
    >
      <div v-if="fullPage" :class="[classes.overlay()]" :tabindex="-1" />
      <UIcon name="crpg:loading" :class="[classes.icon()]" />
    </div>
  </Transition>
</template>
