<script setup lang="ts">
const slots = useSlots()

const columns = computed(() => [
  ...(slots.leftContent ? ['max-content'] : []),
  ...(slots.default ? ['1fr'] : []),
  ...(slots.rightContent ? ['max-content'] : []),
].join(' '))
</script>

<template>
  <div
    class="inline-grid h-max auto-rows-max items-center gap-x-2"
    :style="{ gridTemplateColumns: columns }"
  >
    <slot name="leftContent" />

    <div class="row-span-2">
      <slot />
    </div>

    <slot name="rightContent" />
  </div>
</template>
