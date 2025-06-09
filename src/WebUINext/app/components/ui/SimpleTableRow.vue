<script setup lang="ts">
defineProps<{
  label?: string
  value?: string
  tooltip?: {
    title: string
    description?: string
  }
}>()

const slots = useSlots()
</script>

<template>
  <UTooltip
    :disabled="!tooltip && !slots['tooltip-content']"
    :text="tooltip?.title"
    :ui="{
      content: 'max-w-xl',
    }"
  >
    <UiDataCell class="rounded px-3 py-2.5 hover:bg-neutral-900">
      <slot name="label">
        {{ label }}
      </slot>

      <template #rightContent>
        <div class="text-xs">
          <slot>
            {{ value }}
          </slot>
        </div>
      </template>
    </UiDataCell>

    <template #content>
      <slot name="tooltip-content">
        <div class="prose space-y-3 prose-invert">
          <div v-if="tooltip?.title" class="text-sm">
            {{ tooltip?.title }}
          </div>
          <div v-if="tooltip?.description" class="text-muted" v-html="tooltip?.description" />
        </div>
      </slot>
    </template>
  </UTooltip>
</template>
