<script setup lang="ts">
import type { Platform } from '~/models/platform'

import { PLATFORM } from '~/models/platform'

defineProps<{
  platform: Platform
  platformUserId: number | string
  userName: string
}>()
</script>

<template>
  <a
    v-if="platform === PLATFORM.Steam"
    :href="`https://steamcommunity.com/profiles/${platformUserId}`"
    class="hover:opacity-80"
    target="_blank"
    @click.stop
  >
    <UIcon name="crpg:steam-duotone" class="size-full" />
  </a>

  <!-- TODO: Epic doesn't have a public profile yet https://trello.com/c/FH3mNJ6b/297-profiles -->
  <UIcon
    v-else-if="platform === PLATFORM.EpicGames"
    name="crpg:epic-games"
  />
</template>
