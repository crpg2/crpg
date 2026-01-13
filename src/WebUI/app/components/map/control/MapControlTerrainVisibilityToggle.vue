<script setup lang="ts">
import { LControl } from '@vue-leaflet/vue-leaflet'

defineEmits<{ toggle: [] }>()
</script>

<template>
  <LControl>
    <div class="leaflet-bar">
      <a
        role="button"
        href="#"
        class="flex! items-center justify-center"
        :title="$t('strategus.control.terrainLayerToggle')"
        @click.prevent.stop="$emit('toggle')"
      >
        <UIcon
          name="crpg:layers"
          size="lg"
        />
      </a>
    </div>
  </LControl>
</template>
