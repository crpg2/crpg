<script setup lang="ts">
defineProps<{
  title: string
  description?: string
  validation?: string
}>()

const slots = useSlots()
</script>

<template>
  <div class="prose">
    <slot name="validation">
      <UiTextView v-if="validation" variant="p" class="text-warning">
        {{ validation }}
      </UiTextView>
    </slot>

    <slot name="title">
      <UiTextView
        v-if="title"
        variant="h4"
        tag="h4"
        :class="{ '!mb-4': description || slots.description }"
      >
        {{ title }}
      </UiTextView>
    </slot>

    <slot name="description">
      <div
        v-if="description"
        class="prose"
        v-html="description"
      />
    </slot>
  </div>
</template>
