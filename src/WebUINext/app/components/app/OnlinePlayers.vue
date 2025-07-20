<script setup lang="ts">
import NumberFlow, { continuous } from '@number-flow/vue'

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
    :ui="{
      content: 'space-y-5',
    }"
  >
    <UiDataCell>
      <template #leftContent>
        <div class="relative size-6">
          <UIcon
            name="crpg:online" class="absolute inset-0 size-full text-[var(--color-notification)]"
          />
          <UIcon
            name="crpg:online-ring" class="
              absolute inset-0 size-full animate-ping text-[var(--color-notification)]/50
            "
          />
        </div>
      </template>

      <div v-if="showLabel" data-aq-online-players-count>
        <i18n-t
          scope="global"
          keypath="onlinePlayers.format"
          class="hover:text-highlighted"
        >
          <template #count>
            <NumberFlow
              :value="gameServerStats.total.playingCount"
              :plugins="[continuous]"
              locales="en-US"
              class="min-w-8"
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
          :value="gameServerStats.total.playingCount"
          locales="en-US"
          :format="{ useGrouping: false }"
          will-change
        />
      </div>
    </UiDataCell>

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
