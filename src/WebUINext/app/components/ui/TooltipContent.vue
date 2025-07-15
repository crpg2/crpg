<script setup lang="ts">
defineProps<{
  title: string
  description?: string
}>()

const slots = useSlots()
</script>

<template>
  <div class="prose">
    <slot name="title">
      <h5 v-if="title" :class="{ 'mb-6': description && slots.description }">
        {{ title }}
      </h5>
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
