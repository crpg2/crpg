<script setup lang="ts" generic="T extends AcceptableValue | AcceptableValue[] | undefined">
import type { AcceptableValue, SelectItem } from '@nuxt/ui'

const props = defineProps<{
  label: string
  items: SelectItem[]
  multiple?: boolean
}>()

const modelValue = defineModel<T>()
</script>

<template>
  <USelect
    v-model="modelValue"
    variant="none"
    :multiple="props.multiple ?? true"
    trailing-icon=""
    size="xl"
    :ui="{
      content: 'min-w-fit',
      base: 'px-0 py-0',
      placeholder: 'pointer-events-auto! overflow-visible!',
    }"
    :items="props.items"
  >
    <template #default>
      <UiGridColumnHeaderLabel
        :label="props.label"
        :with-filter="true"
      />
    </template>
  </USelect>
</template>
