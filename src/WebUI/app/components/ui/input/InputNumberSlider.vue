<script setup lang="ts">
import type { InputNumberProps } from '@nuxt/ui'

const { readonly = false } = defineProps<{
  max: number
  min?: number
  readonly?: boolean
  size?: InputNumberProps['size']
}>()

const modelValue = defineModel<number>({ default: 0 })
</script>

<template>
  <div>
    <UInputNumber
      v-model="modelValue"
      :readonly
      class="w-full"
      :min
      :size
      :max
    />
    <USlider
      :model-value="modelValue"
      class="px-2"
      :min
      :max
      :disabled="readonly"
      :size
      @update:model-value="(value) => {
        // TODO: Strange behavior of the nuxt/ui Slider component. If you're not too lazy, please  create an issue/PR
        if (value !== undefined && typeof value === 'number') {
          modelValue = value
        }
      }"
    />
  </div>
</template>
