<script lang="ts" setup>
const { label } = defineProps<{
  label?: string
}>()
</script>

<template>
  <li
    class="ps-2"
  >
    <UiTextView v-if="label" variant="p" tag="p">
      {{ label }}
    </UiTextView>
    <slot v-else />
  </li>
</template>
