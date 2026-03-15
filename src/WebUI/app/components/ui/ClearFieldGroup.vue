<script setup lang="ts">
// TODO: FIXME: https://github.com/nuxt/ui/issues/5045
import { fieldGroupInjectionKey } from '@nuxt/ui/runtime/composables/useFieldGroup.js'
import { computed, provide } from 'vue'

provide(fieldGroupInjectionKey, computed(() => ({
  orientation: undefined,
  size: undefined,
})))
</script>

<template>
  <slot />
</template>
