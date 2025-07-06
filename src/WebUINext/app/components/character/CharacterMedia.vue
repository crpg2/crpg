<script setup lang="ts">
import type { CharacterPublic } from '~/models/character'

import { characterClassToIcon } from '~/services/character-service'

const { character } = defineProps<{
  character: CharacterPublic
}>()
</script>

<template>
  <UiDataCell>
    <template #leftContent>
      <UTooltip :text="$t(`character.class.${character.class}`)">
        <UIcon
          :name="`crpg:${characterClassToIcon[character.class]}`"
          class="size-5"
        />
      </UTooltip>
    </template>

    <span class="max-w-[150px] truncate">
      {{ character.name }}
    </span>
    ({{ character.level }})
  </UiDataCell>
</template>
