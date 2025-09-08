<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'

interface Props {
  to?: RouteLocationRaw
}

defineProps<Props>()
</script>

<template>
  <UButton
    :to
    color="neutral"
    variant="outline"
    size="xl"
    icon="crpg:arrow-left"
  />
</template>
