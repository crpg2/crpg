<script setup lang="ts">
import { tv } from 'tailwind-variants'

const { size = 'md' } = defineProps<{
  value?: number
  size?: 'md' | 'lg' | 'xl'
}>()

const variants = tv({
  slots: {
    icon: '',
    label: 'font-bold text-primary',
  },
  variants: {
    size: {
      md: {
        icon: 'size-4',
        label: '',
      },
      lg: {
        icon: 'size-6',
        label: '',
      },
      xl: {
        icon: 'size-8',
        label: 'text-lg',
      },
    },
  },
})

const classes = computed(() => variants({ size }))
</script>

<template>
  <UTooltip :text="$t('user.field.gold')">
    <UiDataMedia>
      <template #icon>
        <UiSpriteSymbol name="coin" viewBox="0 0 18 18" :class="classes.icon()" />
      </template>
      <template #default>
        <slot>
          <span
            v-if="value !== undefined"
            :class="classes.label()"
          >
            {{ $n(value) }}
          </span>
        </slot>
      </template>
    </UiDataMedia>
  </UTooltip>
</template>
