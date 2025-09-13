<script lang="ts" setup>
const { label } = defineProps<{
  label?: string
}>()
</script>

<template>
  <li class="ps-2">
    <div>
      <slot name="label">
        <p v-if="label">
          {{ label }}
        </p>
      </slot>
    </div>
    <slot />
  </li>
</template>
