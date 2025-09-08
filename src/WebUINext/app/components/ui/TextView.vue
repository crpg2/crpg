<script lang="ts" setup>
import { tv } from 'tailwind-variants'

type Variant = 'h1' | 'h2' | 'h3' | 'p' | 'caption'

const { tag = 'div' } = defineProps<{
  tag?: string
  variant: Variant
}>()

const variants = tv({
  variants: {
    variant: {
      h1: 'h1',
      h2: 'h2',
      h3: 'h3',
      p: 'p',
      caption: 'caption',
    },
  },
})
</script>

<template>
  <component
    :is="tag"
    :class="variants({ variant })"
  >
    <slot />
  </component>
</template>
