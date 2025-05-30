<script setup lang="ts">
import { useTransition } from '@vueuse/core'

import type { GameServerStats } from '~/models/game-server-stats'

import { gameModeToIcon } from '~/services/game-mode-service'

const { gameServerStats, showLabel = false } = defineProps<{
  gameServerStats: GameServerStats
  showLabel?: boolean
}>()

const animatedPlayingCount = useTransition(() => gameServerStats.total.playingCount)
</script>

<template>
  <UPopover
    arrow
    mode="hover"
    :open-delay="300"
    :close-delay="100"
    :disabled="gameServerStats === null || Object.keys(gameServerStats.regions).length === 0"
  >
    <div class="flex items-center gap-1.5 select-none hover:text-content-100">
      <div class="relative size-6">
        <UIcon name="crpg:online" class="absolute inset-0 size-full text-[#53BC96]" />
        <UIcon name="crpg:online-ring" class="absolute inset-0 size-full animate-ping text-[#53BC96]/50" />
      </div>

      <div v-if="showLabel" data-aq-online-players-count>
        {{ $t('onlinePlayers.format', {
          count: $n(Number(animatedPlayingCount.toFixed(0))),
        }) }}
      </div>
      <div
        v-else
        class="w-8"
        data-aq-online-players-count
      >
        {{ $n(Number(animatedPlayingCount.toFixed(0))) }}
      </div>
    </div>

    <template #content>
      <div class="prose space-y-5 prose-invert">
        <h5 class="text-content-100">
          {{ $t('onlinePlayers.tooltip.title') }}
        </h5>

        <div v-if="gameServerStats !== null" class="space-y-6" data-aq-region-stats>
          <div
            v-for="(regionServerStats, regionKey) in gameServerStats.regions" :key="regionKey"
            class="flex flex-col gap-3"
          >
            <div class="text-white">
              {{ $t(`region.${regionKey}`, 0) }}
            </div>

            <div
              v-for="(regionServerMode, gameModeKey) in regionServerStats" :key="gameModeKey"
              class="flex w-44 items-center justify-between gap-2"
            >
              <div class="flex items-center gap-2">
                <OIcon size="xl" :icon="gameModeToIcon[gameModeKey]" />
                <div>{{ $t(`game-mode.${gameModeKey}`) }}</div>
              </div>
              <div class="text-white">
                {{ regionServerMode!.playingCount }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </template>
  </UPopover>
</template>
