<script setup lang="ts">
import NumberFlow from '@number-flow/vue'

import type { GameServerStats } from '~/models/game-server-stats'

import { gameModeToIcon } from '~/services/game-mode-service'

const { gameServerStats, showLabel = false } = defineProps<{
  gameServerStats: GameServerStats
  showLabel?: boolean
}>()
</script>

<template>
  <UPopover
    arrow
    mode="hover"
    :open-delay="300"
    :close-delay="100"
    :disabled="gameServerStats === null || Object.keys(gameServerStats.regions).length === 0"
  >
    <UButton variant="link" color="neutral" size="xl">
      <template #leading>
        <UiPingIcon />
      </template>

      <div v-if="showLabel" data-aq-online-players-count>
        <i18n-t
          scope="global"
          keypath="onlinePlayers.format"
          class="hover:text-highlighted"
        >
          <template #count>
            <NumberFlow :value="gameServerStats.total.playingCount" />
          </template>
        </i18n-t>
      </div>

      <div
        v-else
        data-aq-online-players-count
      >
        <NumberFlow :value="gameServerStats.total.playingCount" />
      </div>
    </UButton>

    <template #content>
      <UiTextView variant="h4" margin-bottom>
        {{ $t('onlinePlayers.tooltip.title') }}
      </UiTextView>

      <div
        v-if="gameServerStats !== null"
        class="space-y-6"
        data-aq-region-stats
      >
        <div
          v-for="(regionServerStats, regionKey) in gameServerStats.regions" :key="regionKey"
          class="flex flex-col gap-2"
        >
          <UiTextView variant="h5" class="text-highlighted">
            {{ $t(`region.${regionKey}`, 0) }}
          </UiTextView>

          <UiDataCell
            v-for="(regionServerMode, gameModeKey) in regionServerStats" :key="gameModeKey"
            class="min-w-44"
          >
            <template #leftContent>
              <UIcon :name="`crpg:${gameModeToIcon[gameModeKey]}`" class="size-6" />
            </template>

            <UiTextView variant="p-sm">
              {{ $t(`game-mode.${gameModeKey}`) }}
            </UiTextView>

            <template #rightContent>
              <UiTextView variant="h5">
                {{ regionServerMode!.playingCount }}
              </UiTextView>
            </template>
          </UiDataCell>
        </div>
      </div>
    </template>
  </UPopover>
</template>
