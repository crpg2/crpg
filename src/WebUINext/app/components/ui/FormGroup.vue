<script setup lang="ts">
import { tv } from 'tailwind-variants'

// TODO: RENAME component
type Variant = 'primary' | 'danger'

const {
  bordered = true,
  collapsable = true,
  collapsed = false,
  variant = 'primary',
} = defineProps<{
  variant?: Variant
  icon?: string
  label?: string
  collapsed?: boolean
  collapsable?: boolean
  bordered?: boolean
}>()

const collapsedModel = ref<boolean>(collapsed)

const variants = tv({
  slots: {
    wrapper: '',
    icon: '',
    label: '',
  },
  variants: {
    variant: {
      primary: {
        wrapper: 'border-border-200',
        icon: 'text-content-400 group-hover:text-content-100',
        label: '',
      },
      danger: {
        wrapper: 'border-2 border-dashed border-status-danger',
        icon: 'text-status-danger/80 group-hover:text-status-danger/100',
        label: 'text-status-danger',
      },
    },
    bordered: {
      true: {
        wrapper: 'border',
      },
    },
  },
})

const classes = computed(() => variants({ variant, bordered }))
</script>

<template>
  <div
    class="rounded-3xl px-6 py-7"
    :class="classes.wrapper()"
  >
    <div
      class="group flex items-center justify-between gap-4 text-content-100"
      :class="{ 'cursor-pointer': collapsable }"
      @click="collapsable && (collapsedModel = !collapsedModel)"
    >
      <div
        class="flex w-full items-center gap-3 text-title-lg"
        :class="classes.label()"
      >
        <OIcon
          v-if="icon"
          :icon
          size="lg"
        />
        <slot name="label">
          {{ label }}
        </slot>
      </div>

      <OIcon
        v-if="collapsable"
        icon="chevron-down"
        size="lg"
        :class="[{ 'rotate-180': !collapsedModel }, classes.icon()]"
      />
    </div>

    <div
      v-if="!collapsedModel"
      class="mt-7 flex flex-col gap-4"
    >
      <slot name="default" />
    </div>
  </div>
</template>
