<script setup lang="ts">
import type { CharacterPublic } from '~/models/character'

import { characterClassToIcon } from '~/services/character-service'

const { character, hiddenName = false, hiddenLevel = false } = defineProps<{
  character: CharacterPublic & { name?: string }
  hiddenName?: boolean
  hiddenLevel?: boolean
}>()
</script>

<template>
  <UiDataMedia>
    <template #icon="{ classes: iconClasses }">
      <UTooltip :text="$t(`character.class.${character.class}`)">
        <UIcon
          :name="`crpg:${characterClassToIcon[character.class]}`"
          :class="iconClasses()"
        />
      </UTooltip>
    </template>

    <template v-if="!hiddenName || !hiddenLevel" #default>
      <div class="flex gap-1">
        <div v-if="!hiddenName && character.name" class="max-w-[150px] truncate">
          {{ character.name }}
        </div>
        <span v-if="!hiddenLevel">
          ({{ character.level }})
        </span>
      </div>
    </template>
  </UiDataMedia>
</template>
