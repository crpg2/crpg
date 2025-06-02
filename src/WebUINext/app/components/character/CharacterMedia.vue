<script setup lang="ts">
import type { CharacterPublic } from '~/models/character'

import { characterClassToIcon } from '~/services/character-service'

const { character, isActive = false, forTournament = false } = defineProps<{
  character: CharacterPublic
  isActive?: boolean
  forTournament?: boolean
}>()
</script>

<template>
  <div class="flex items-center gap-2">
    <UTooltip :text="$t(`character.class.${character.class}`)">
      <UIcon
        :name="`crpg:${characterClassToIcon[character.class]}`"
        class="size-6"
      />
    </UTooltip>

    <div class="flex items-center gap-1">
      <span class="max-w-[150px] overflow-hidden text-ellipsis whitespace-nowrap">
        {{ character.name }}
      </span>
      <span>({{ character.level }})</span>
    </div>

    <UTooltip v-if="isActive" :text="$t('character.status.active.title')">
      <UBadge
        :label="$t('character.status.active.short')"
        color="success"
        variant="soft"
        size="sm"
      />
    </UTooltip>

    <UiTag
      v-if="forTournament"
      v-tooltip="$t('character.status.forTournament.title')"
      :label="$t('character.status.forTournament.short')"
      variant="warning"
      size="sm"
    />
  </div>
</template>
