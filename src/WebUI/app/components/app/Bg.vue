<script setup lang="ts">
const { strategy = 'fixed' } = defineProps<{ bg: string, strategy?: 'absolute' | 'fixed' }>()
</script>

<template>
  <div class="inset-0" :class="strategy === 'fixed' ? 'fixed' : 'absolute'">
    <UiImg
      class="size-full object-cover opacity-15"
      :src="`/images/bg/${bg}`"
    />
    <!-- <div class="absolute inset-0 bg-default/50 backdrop-blur-[0.075rem]" /> -->
    <div class="absolute inset-0 bg-linear-to-t from-default opacity-75 backdrop-blur-[0.075rem]" />
  </div>
</template>
