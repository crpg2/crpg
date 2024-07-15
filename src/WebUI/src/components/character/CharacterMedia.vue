<script setup lang="ts">
import type { CharacterPublic } from '~/models/character'

import { characterClassToIcon } from '~/services/characters-service'

const {
  character,
  isActive = false,
  forTournament = false,
} = defineProps<{
  character: CharacterPublic
  isActive?: boolean
  forTournament?: boolean
}>()
</script>

<template>
  <div class="flex items-center gap-1.5">
    <OIcon
      v-tooltip="$t(`character.class.${character.class}`)"
      :icon="characterClassToIcon[character.class]"
      size="lg"
    />

    <div class="flex items-center gap-1">
      <div class="max-w-[150px] overflow-x-hidden text-ellipsis whitespace-nowrap">
        {{ character.name }}
      </div>

      <div>({{ character.level }})</div>
    </div>

    <Tag
      v-if="isActive"
      v-tooltip="$t('character.status.active.title')"
      :label="$t('character.status.active.short')"
      variant="success"
      size="sm"
    />

    <Tag
      v-if="forTournament"
      v-tooltip="$t('character.status.forTournament.title')"
      :label="$t('character.status.forTournament.short')"
      variant="warning"
      size="sm"
    />
  </div>
</template>
