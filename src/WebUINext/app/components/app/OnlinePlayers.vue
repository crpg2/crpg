<script setup lang="ts">
import NumberFlow, { continuous } from '@number-flow/vue'
import { useTransition } from '@vueuse/core'

import type { GameServerStats } from '~/models/game-server-stats'

import { gameModeToIcon } from '~/services/game-mode-service'

const { gameServerStats, showLabel = false } = defineProps<{
  gameServerStats: GameServerStats
  showLabel?: boolean
}>()

const animatedPlayingCount = useTransition(() => gameServerStats.total.playingCount)

const d = ref(2)

setInterval(() => {
  d.value = Math.ceil((Math.random() * 11 + 2))
}, 1000)
</script>

<template>
  <UPopover
    arrow
    mode="hover"
    :open-delay="300"
    :close-delay="100"
    :disabled="gameServerStats === null || Object.keys(gameServerStats.regions).length === 0"
    :ui="{
      content: 'space-y-5',
    }"
  >
    <div
      class="
        flex items-center gap-1.5 select-none
        hover:text-content-100
      "
    >
      <!-- {{ gameServerStats }} -->
      <div class="relative size-6">
        <UIcon name="crpg:online" class="absolute inset-0 size-full text-[#53BC96]" />
        <UIcon
          name="crpg:online-ring" class="absolute inset-0 size-full animate-ping text-[#53BC96]/50"
        />
      </div>

      <div v-if="showLabel" data-aq-online-players-count>
        <i18n-t
          scope="global"
          keypath="onlinePlayers.format"
          tag="span"
          class="text-highlighted"
        >
          <template #count>
            <NumberFlow
              :value="d"
              :plugins="[continuous]"
              locales="en-US"
              class="min-w-[30px]"
              will-change
            />
          </template>
        </i18n-t>
      </div>
      <div
        v-else
        class="w-8"
        data-aq-online-players-count
      >
        <NumberFlow
          :value="d"
          locales="en-US"
          :format="{ useGrouping: false }"
          aria-hidden="true"
          will-change
        />
        <!-- {{ $n(Number(animatedPlayingCount.toFixed(0))) }} -->
      </div>
    </div>

    <template #content>
      <div class="text-sm text-highlighted">
        {{ $t('onlinePlayers.tooltip.title') }}
      </div>

      <div v-if="gameServerStats !== null" class="space-y-6" data-aq-region-stats>
        <div
          v-for="(regionServerStats, regionKey) in gameServerStats.regions" :key="regionKey"
          class="flex flex-col gap-2"
        >
          <div class="text-highlighted">
            {{ $t(`region.${regionKey}`, 0) }}
          </div>

          <UiDataCell
            v-for="(regionServerMode, gameModeKey) in regionServerStats" :key="gameModeKey"
            class="min-w-44"
          >
            <template #leftContent>
              <UIcon :name="`crpg:${gameModeToIcon[gameModeKey]}`" class="size-6" />
            </template>

            <div>{{ $t(`game-mode.${gameModeKey}`) }}</div>

            <template #rightContent>
              <div class="text-highlighted">
                {{ regionServerMode!.playingCount }}
              </div>
            </template>
          </UiDataCell>
        </div>
      </div>
    </template>
  </UPopover>
</template>
