<script lang="ts" setup>
import { tv } from 'tailwind-variants'

type Variant = 'bullet' | 'number'

const {
  variant = 'bullet',
  marginBottom = true,
} = defineProps<{
  variant?: Variant
  marginBottom?: boolean
}>()

const variants = tv({
  base: `
    m-0 space-y-2 p-0 ps-7
    marker:text-toned
  `,
  variants: {
    variant: {
      bullet: 'list-disc',
      number: 'list-decimal',
    },
    marginBottom: {
      true: 'mb-2',
      false: '',
    },
  },
})

const tag = computed(() => variant === 'bullet' ? 'ul' : 'ol')
</script>

<template>
  <component
    :is="tag"
    :class="variants({ variant, marginBottom })"
  >
    <slot />
  </component>
</template>
