<script setup lang="ts">
</script>

<template>
  <div>
    <UCard
      variant="soft"
      :ui="{
        header: 'pt-6 pb-3',
        footer: 'pb-6 pt-3',
        body: 'flex-1 overflow-x-hidden overflow-y-auto py-3',
        root: 'bg-elevated/80 h-full backdrop-blur-sm flex flex-col divide-muted',
      }"
    >
      <template #header>
        <slot name="header" />
      </template>

      <slot />

      <template #footer>
        <slot name="footer" />
      </template>
    </UCard>
  </div>
</template>
