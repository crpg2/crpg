<script setup lang="ts">
import { Platform } from '~/models/platform'

const { size = 'sm' } = defineProps<{
  platform: Platform
  platformUserId: number | string
  userName: string
  size?: 'sm' | 'xl'
}>()
</script>

<template>
  <template v-if="platform === Platform.Steam">
    <a
      :href="`https://steamcommunity.com/profiles/${platformUserId}`"
      class="inline-flex hover:opacity-80"
      target="_blank"
      @click.stop
    >
      <OIcon
        icon="steam-duotone"
        :size="size"
        :style="{
          '--fa-secondary-opacity': 1,
          '--fa-secondary-color': '#2F3944',
          '--fa-primary-color': '#fff',
        }"
      />
    </a>
  </template>

  <!-- TODO: Epic doesn't have a public profile yet https://trello.com/c/FH3mNJ6b/297-profiles -->
  <template v-else-if="platform === Platform.EpicGames">
    <OIcon
      icon="epic-games"
      :size="size"
    />
  </template>

  <template v-else-if="platform === Platform.Microsoft">
    <a
      :href="`https://account.xbox.com/en-us/profile?gamertag=${userName}`"
      class="inline-flex hover:opacity-80"
      target="_blank"
      @click.stop
    >
      <OIcon
        icon="xbox"
        :size="size"
      />
    </a>
  </template>
</template>
