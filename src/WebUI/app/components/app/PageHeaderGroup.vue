<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'

defineProps<{
  title: string
  backTo?: RouteLocationRaw
}>()
</script>

<template>
  <div class="grid grid-cols-3 gap-5">
    <div>
      <AppBackButton v-if="backTo" :to="backTo" />
    </div>

    <UiTextView
      variant="h1"
      tag="h1"
      class="text-center"
    >
      {{ title }}
    </UiTextView>
  </div>
</template>
