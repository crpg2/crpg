<script setup lang="ts">
import { tv } from 'tailwind-variants'

type BadgeVariant = 'primary' | 'info' | 'success' | 'warning' | 'danger'
type BadgeSize = 'sm' | 'lg'

const {
  disabled = false,
  padded = true,
  rounded = false,
  size = 'sm',
  variant = 'info',
} = defineProps<{
  variant?: BadgeVariant
  size?: BadgeSize
  rounded?: boolean
  disabled?: boolean
  padded?: boolean
  label?: string
  icon?: string
}>()

const variants = tv({
  base: '!inline-flex cursor-pointer items-center justify-center gap-1 bg-base-200',
  variants: {
    variant: {
      primary: 'text-primary hover:bg-base-300 hover:text-primary-hover',
      success: 'text-status-success hover:bg-status-success hover:text-content-600',
      warning: 'text-status-warning hover:bg-status-warning hover:text-content-600',
      info: 'text-content-200 hover:bg-base-500 hover:text-content-100',
      danger: 'text-status-danger hover:bg-status-danger hover:text-content-600',
    },
    disabled: {
      true: 'pointer-events-none',
    },
    rounded: {
      true: 'rounded-full',
      false: 'rounded-md',
    },
    padded: {
      true: '',
    },
    size: {
      lg: 'text-2xs',
      sm: 'text-3xs',
    },
  },
  compoundVariants: [
    {
      padded: true,
      rounded: true,
      size: 'lg',
      class: 'size-7',
    },
    {
      padded: true,
      rounded: true,
      size: 'sm',
      class: 'size-5',
    },
    {
      padded: true,
      rounded: false,
      size: 'lg',
      class: 'px-1.5 py-0.5',
    },
    {
      padded: true,
      rounded: false,
      size: 'sm',
      class: 'px-1.5 py-0.5',
    },
  ],
}, { twMerge: false })

const iconSize = computed(() => {
  return ({ lg: 'sm', sm: 'xs' } satisfies Record<BadgeSize, string>)[size]
})
</script>

<template>
  <div
    :class="variants({
      size,
      variant,
      disabled,
      padded,
      rounded,
    })"
  >
    <OIcon
      v-if="icon"
      :icon="icon"
      :size="iconSize"
    />
    <template v-if="label">
      {{ label }}
    </template>
  </div>
</template>
