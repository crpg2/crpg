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
    :ui="{
      content: 'max-w-sm',
    }"
  >
    <UiDataCell
      class="
        group px-3 py-2.5
        hover:bg-muted
      "
    >
      <div class="text-muted">
        <slot name="label">
          {{ label }}
        </slot>
      </div>

      <template #rightContent>
        <div
          class="
            font-bold
            group-hover:text-highlighted
          "
        >
          <slot>
            {{ value }}
          </slot>
        </div>
      </template>
    </UiDataCell>

    <template #content>
      <slot name="tooltip-content">
        <UiTooltipContent
          v-if="tooltip"
          v-bind="{ ...tooltip }"
        />
      </slot>
    </template>
  </UTooltip>
</template>
