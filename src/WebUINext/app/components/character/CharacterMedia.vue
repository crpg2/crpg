<script setup lang="ts">
import type { CharacterPublic } from '~/models/character'

import { characterClassToIcon } from '~/services/character-service'

const { character } = defineProps<{
  character: CharacterPublic
}>()
</script>

<template>
  <UiDataMedia :icon="`crpg:${characterClassToIcon[character.class]}`">
    <template #default>
      <div class="flex gap-1">
        <div class="max-w-[150px] truncate">
          {{ character.name }}
        </div>
        ({{ character.level }})
      </div>
    </template>
  </UiDataMedia>
</template>
