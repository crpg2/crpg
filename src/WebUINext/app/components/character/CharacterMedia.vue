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
  <!-- <div class="flex items-center gap-2">
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
  </div> -->
</template>
