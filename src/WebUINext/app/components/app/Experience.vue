<script setup lang="ts">
import { tv } from 'tailwind-variants'

const { size = 'md' } = defineProps<{
  value?: number
  size?: 'md' | 'lg' | 'xl'
}>()

const variants = tv({
  slots: {
    icon: 'text-primary',
    label: 'font-bold',
  },
  variants: {
    size: {
      md: {
        icon: 'size-5',
        label: '',
      },
      lg: {
        icon: 'size-7',
        label: '',
      },
      xl: {
        icon: 'size-10',
        label: 'text-lg',
      },
    },
  },
})

const classes = computed(() => variants({ size }))
</script>

<template>
  <UiDataCell>
    <template #leftContent>
      <!-- TODO: sandbox -->
      <!-- <div class="flex h-12 items-center">
      </div> -->
      <UIcon
        name="crpg:experience"
        :class="classes.icon()"
      />
    </template>
    <slot>
      <span v-if="value !== undefined" :class="classes.label()">
        {{ $n(value) }}
      </span>
    </slot>
  </UiDataCell>
</template>
