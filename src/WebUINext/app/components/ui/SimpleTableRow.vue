<script setup lang="ts">
defineProps<{
  label?: string
  value?: string
  tooltip?: {
    title: string
    description?: string
  }
}>()
</script>

<template>
  <UTooltip
    :disabled="!tooltip"
    :text="tooltip?.title"
    :ui="{
      content: 'max-w-xl z-[1000]',
    }"
  >
    <UiDataCell class="rounded px-3 py-2.5 hover:bg-base-200">
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
      <div class="prose space-y-3 prose-invert">
        <div v-if="tooltip?.title" class="text-sm">
          {{ tooltip?.title }}
        </div>
        <div v-if="tooltip?.description" class="text-muted" v-html="tooltip?.description" />
      </div>
    </template>
  </UTooltip>
</template>
