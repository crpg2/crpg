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
  <UTooltip :disabled="!tooltip && !slots['tooltip-content']">
    <UiDataCell
      class="
        group rounded px-3 py-2.5
        hover:bg-muted
      "
    >
      <div
        class="
          text-sm text-muted
          group-hover:text-default
        "
      >
        <slot name="label">
          {{ label }}
        </slot>
      </div>

      <template #rightContent>
        <div
          class="
            font-semibold
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
