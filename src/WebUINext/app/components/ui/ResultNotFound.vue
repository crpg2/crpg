<script setup lang="ts">
defineProps<{ message?: string }>()
</script>

<template>
  <div class="flex flex-col items-center justify-center gap-4 p-6 text-center text-dimmed xl:p-24">
    <UIcon
      name="crpg:not-found"
      class="size-10"
    />
    <div class="text-xs">
      {{ message ?? $t('results-not-found') }}
    </div>
  </div>
</template>
