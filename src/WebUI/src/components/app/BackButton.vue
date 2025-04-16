<script setup lang="ts">
import type { RouterLinkProps } from 'vue-router'

const props = defineProps<RouterLinkProps>()
</script>

<template>
  <RouterLink v-bind="props">
    <OButton
      v-tooltip.bottom="$t('nav.back')"
      variant="secondary"
      size="xl"
      outlined
      rounded
      icon-left="arrow-left"
    />
  </RouterLink>
</template>
