<script setup lang="ts">
</script>

<template>
  <UCard
    variant="subtle"
    :ui="{
      header: 'pt-6 pb-3',
      footer: 'pb-6 pt-3',
      body: 'flex-1 overflow-x-hidden overflow-y-auto sm:py-3',
      root: 'ring-2 ring-accented bg-elevated/90 h-full backdrop-blur-md flex flex-col divide-muted',
    }"
  >
    <template #header>
      <slot name="header" />
    </template>

    <slot />

    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </UCard>
</template>
