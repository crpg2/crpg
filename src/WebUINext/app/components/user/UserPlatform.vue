<script setup lang="ts">
import { Platform } from '~/models/platform'

defineProps<{
  platform: Platform
  platformUserId: number | string
  userName: string
}>()
</script>

<template>
  <a
    v-if="platform === Platform.Steam"
    :href="`https://steamcommunity.com/profiles/${platformUserId}`"
    class="inline-flex hover:opacity-80"
    target="_blank"
    @click.stop
  >
    <UIcon name="crpg:steam-duotone" class="size-5" />
  </a>

  <!-- TODO: Epic doesn't have a public profile yet https://trello.com/c/FH3mNJ6b/297-profiles -->
  <UIcon
    v-else-if="platform === Platform.EpicGames"
    name="crpg:epic-games"
    class="size-5"
  />

  <a
    v-else-if="platform === Platform.Microsoft"
    :href="`https://account.xbox.com/en-us/profile?gamertag=${userName}`"
    class="inline-flex hover:opacity-80"
    target="_blank"
    @click.stop
  >
    <UIcon name="crpg:xbox" class="size-5" />
  </a>
</template>
