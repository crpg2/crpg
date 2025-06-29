<script setup lang="ts">
import type { NuxtLinkProps } from '#app'

defineProps<NuxtLinkProps>()
</script>

<template>
  <UButton
    :to
    color="secondary"
    variant="outline"
    size="xl"
    icon="crpg:arrow-left"
  />
</template>
