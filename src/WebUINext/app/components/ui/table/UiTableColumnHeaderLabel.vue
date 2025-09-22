<script setup lang="ts">
const { withFilter = false } = defineProps<{
  label: string
  description?: string
  withFilter?: boolean
}>()
</script>

<template>
  <UTooltip
    :disabled="!description"
  >
    <div
      class="text-xs text-default"
      :class="{ 'cursor-pointer underline decoration-dashed underline-offset-6 select-none hover:no-underline': withFilter }"
    >
      {{ label }}
    </div>

    <template #content>
      <UiTooltipContent :title="label" :description />
    </template>
  </UTooltip>
</template>
<!--
 // return h(UTooltip, {
      //   delayDuration: 600,
      //   disabled: !description,
      //   ui: {
      //     content: 'max-w-sm',
      //   },
      // }, {
      //   default: () => h('div', null, t(`item.aggregations.${header.id}.title`)),
      //   content: () => h('div', { class: 'prose prose-invert' }, [
      //     h('h4', null, t(`item.aggregations.${header.id}.title`)),
      //     h('div', { innerHTML: description }),
      //   ]),
      // }) -->
