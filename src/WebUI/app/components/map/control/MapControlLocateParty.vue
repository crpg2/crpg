<script setup lang="ts">
import { LControl } from '@vue-leaflet/vue-leaflet'

defineEmits<{ click: [] }>()
</script>

<template>
  <LControl>
    <div class="leaflet-bar">
      <a
        href="#"
        class="flex! items-center justify-center"
        :title="$t('strategus.control.locateParty')"
        @click="$emit('click')"
      >
        <UIcon
          name="i-lucide-locate-fixed"
          class="size-4"
        />
      </a>
    </div>
  </LControl>
</template>
