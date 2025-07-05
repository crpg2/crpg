<script setup lang="ts">
import { tv } from 'tailwind-variants'

import type { Platform } from '~/models/platform'

import { PLATFORM } from '~/models/platform'

type Size = 'sm' | 'md' | 'lg' | 'xl'

const { size = 'md' } = defineProps<{
  platform: Platform
  platformUserId: number | string
  userName: string
  size?: Size
}>()

const variants = tv({
  slots: {
    icon: '',
  },
  variants: {
    size: {
      sm: {
        icon: '',
      },
      md: {
        icon: 'size-5',
      },
      lg: {
        icon: 'size-6',
      },
      xl: {
        icon: 'size-8',
      },
    },
  },
})

const classes = computed(() => variants({ size }))
</script>

<template>
  <a
    v-if="platform === PLATFORM.Steam"
    :href="`https://steamcommunity.com/profiles/${platformUserId}`"
    class="
      inline-flex
      hover:opacity-80
    "
    target="_blank"
    @click.stop
  >
    <UIcon name="crpg:steam-duotone" :class="classes.icon()" />
  </a>

  <!-- TODO: Epic doesn't have a public profile yet https://trello.com/c/FH3mNJ6b/297-profiles -->
  <UIcon
    v-else-if="platform === PLATFORM.EpicGames"
    name="crpg:epic-games"
    :class="classes.icon()"
  />

  <a
    v-else-if="platform === PLATFORM.Microsoft"
    :href="`https://account.xbox.com/en-us/profile?gamertag=${userName}`"
    class="
      inline-flex
      hover:opacity-80
    "
    target="_blank"
    @click.stop
  >
    <UIcon name="crpg:xbox" :class="classes.icon()" />
  </a>
</template>
