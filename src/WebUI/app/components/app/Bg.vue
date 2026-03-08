<script setup lang="ts">
const { strategy = 'fixed' } = defineProps<{ bg: string, strategy?: 'absolute' | 'fixed' }>()

const colorMode = useColorMode()
</script>

<template>
  <div class="inset-0" :class="strategy === 'fixed' ? 'fixed' : 'absolute'">
    <UiImg
      class="size-full object-cover opacity-50"
      :class="colorMode.value === 'dark' ? 'brightness-75' : 'brightness-125'"
      :src="`/images/bg/${bg}`"
    />
    <div class="absolute inset-0 bg-default/50 backdrop-blur-[0.075rem]" />
  </div>
</template>
