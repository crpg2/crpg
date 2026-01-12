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
          name="crpg:crosshair"
          size="lg"
        />
      </a>
    </div>
  </LControl>
</template>
