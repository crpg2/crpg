<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'

const { decorated = false } = defineProps<{
  title: string
  decorated?: boolean
  backTo?: RouteLocationRaw
}>()
</script>

<template>
  <div class="grid grid-cols-3 gap-5">
    <div>
      <AppBackButton v-if="backTo" :to="backTo" />
    </div>

    <UiHeading v-if="decorated" :title="title" />

    <UiTextView
      v-else
      variant="h1"
      tag="h1"
      class="text-center"
    >
      {{ title }}
    </UiTextView>
  </div>
</template>
