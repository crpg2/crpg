<script lang="ts" setup>
import { tv } from 'tailwind-variants'

type Variant = 'h1' | 'h2' | 'h3' | 'h4' | 'h5' | 'p' | 'p-sm' | 'p-xs' | 'caption' | 'caption-sm' | 'caption-xs'

const { tag = 'div', marginTop = false, marginBottom = false } = defineProps<{
  tag?: string
  variant: Variant
  marginTop?: boolean
  marginBottom?: boolean
}>()

const variants = tv({
  variants: {
    variant: {
      //
      'h1': 'h1',
      'h2': 'h2',
      'h3': 'h3',
      'h4': 'h4',
      'h5': 'h5',
      //
      'p': 'text-base',
      'p-sm': 'text-sm',
      'p-xs': 'text-xs',
      //
      'caption': 'text-dimmed',
      'caption-sm': 'text-sm text-dimmed',
      'caption-xs': 'text-xs text-dimmed',
    },
    marginTop: {
      true: '',
      false: '',
    },
    marginBottom: {
      true: '',
      false: '',
    },
  },
  compoundVariants: [
    {
      variant: 'h1',
      marginTop: true,
      class: 'mt-12',
    },
    {
      variant: 'h1',
      marginBottom: true,
      class: 'mb-8',
    },
    {
      variant: 'h2',
      marginTop: true,
      class: 'mt-12',
    },
    {
      variant: 'h2',
      marginBottom: true,
      class: 'mb-6',
    },
    {
      variant: 'h3',
      marginTop: true,
      class: 'mt-8',
    },
    {
      variant: 'h3',
      marginBottom: true,
      class: 'mb-3',
    },
    {
      variant: 'h4',
      marginTop: true,
      class: 'mt-6',
    },
    {
      variant: 'h4',
      marginBottom: true,
      class: 'mb-2',
    },
    {
      variant: 'h5',
      marginTop: true,
      class: 'mt-5',
    },
    {
      variant: 'h5',
      marginBottom: true,
      class: 'mb-1',
    },
    {
      variant: 'p',
      marginTop: true,
      class: 'mt-5',
    },
    {
      variant: 'p',
      marginBottom: true,
      class: 'mb-5',
    },
    {
      variant: 'p-sm',
      marginTop: true,
      class: 'mt-5',
    },
    {
      variant: 'p-sm',
      marginBottom: true,
      class: 'mb-5',
    },
    {
      variant: 'p-xs',
      marginTop: true,
      class: 'mt-5',
    },
    {
      variant: 'p-xs',
      marginBottom: true,
      class: 'mb-5',
    },
    {
      variant: 'caption',
      marginTop: true,
      class: 'mt-5',
    },
    {
      variant: 'caption',
      marginBottom: true,
      class: 'mb-5',
    },
    {
      variant: 'caption-sm',
      marginTop: true,
      class: 'mt-4',
    },
    {
      variant: 'caption-sm',
      marginBottom: true,
      class: 'mb-4',
    },
    {
      variant: 'caption-xs',
      marginTop: true,
      class: 'mt-3',
    },
    {
      variant: 'caption-xs',
      marginBottom: true,
      class: 'mb-3',
    },
  ],
})
</script>

<template>
  <component
    :is="tag"
    :class="variants({ variant, marginBottom, marginTop })"
  >
    <slot />
  </component>
</template>
