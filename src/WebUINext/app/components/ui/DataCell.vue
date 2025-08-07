<script setup lang="ts">
import { tv } from 'tailwind-variants'

const { inline = false } = defineProps<{
  inline?: boolean
}>()

const slots = useSlots()

const columns = computed(() => [
  ...(slots.leftContent ? ['max-content'] : []),
  ...(slots.default ? ['1fr'] : []),
  ...(slots.rightContent ? ['max-content'] : []),
].join(' '))

const variants = tv({
  slots: {
    root: '',
  },
  variants: {
    inline: {
      true: {
        root: 'inline-grid align-bottom',
      },
      false: {
        root: 'grid',
      },
    },
  },
})

const classes = computed(() => variants({
  inline,
}))
</script>

<template>
  <div
    class="h-max auto-rows-max items-center gap-x-2"
    :class="classes.root()"
    :style="{ gridTemplateColumns: columns }"
  >
    <slot name="leftContent" />

    <div class="row-span-2">
      <slot />
    </div>

    <slot name="rightContent" />
  </div>
</template>
