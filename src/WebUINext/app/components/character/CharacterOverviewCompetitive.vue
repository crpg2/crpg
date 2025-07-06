<script setup lang="ts">
import type { CharacterStatistics } from '~/models/character'
import type { GameMode } from '~/models/game-mode'

import { GAME_MODE } from '~/models/game-mode'
import { getCharacterKDARatio, getDefaultCharacterStatistics } from '~/services/character-service'
import { checkIsRankedGameMode, gameModeToIcon } from '~/services/game-mode-service'
import { msToHours } from '~/utils/date'

const { characterStatistics } = defineProps<{
  characterStatistics: Partial<Record<GameMode, CharacterStatistics>>
}>()

const gameMode = ref<GameMode>(GAME_MODE.CRPGBattle)
const isRankedGameMode = computed(() => checkIsRankedGameMode(gameMode.value))

const { rankTable } = useRankTable()

const gameModeCharacterStatistics = computed(
  () => characterStatistics[gameMode.value] || getDefaultCharacterStatistics(),
)

const kdaRatio = computed(() =>
  gameModeCharacterStatistics.value.deaths === 0
    ? '∞'
    : getCharacterKDARatio(gameModeCharacterStatistics.value),
)
</script>

<template>
  <div class="space-y-6">
    <div class="flex flex-wrap gap-1.5">
      <UButton
        v-for="gm in Object.values(GAME_MODE)"
        :key="gm"
        color="secondary"
        :variant="gm === gameMode ? 'solid' : 'soft'"
        :icon="`crpg:${gameModeToIcon[gm]}`"
        :label="$t(`game-mode.${gm}`, 0)"
        @click="gameMode = gm"
      />
    </div>

    <div class="grid grid-cols-2 gap-2">
      <UiSimpleTableRow
        :label="$t('character.statistics.kda.title')"
        :value="$t('character.format.kda', {
          kills: gameModeCharacterStatistics.kills,
          deaths: gameModeCharacterStatistics.deaths,
          assists: gameModeCharacterStatistics.assists,
          ratio: kdaRatio,
        })"
        :tooltip="{ title: $t('character.statistics.kda.tooltip.title') }"
      />

      <UiSimpleTableRow
        :label="$t('character.statistics.playTime.title')"
        :value="$t('dateTimeFormat.hh', { hours: msToHours(gameModeCharacterStatistics.playTime) })"
      />

      <UiSimpleTableRow
        v-if="isRankedGameMode"
        :tooltip="{
          title: $t('character.statistics.rank.tooltip.title'),
          description: $t('character.statistics.rank.tooltip.desc') }"
      >
        <template #label>
          <div class="flex items-center gap-1.5">
            {{ $t('character.statistics.rank.title') }}
            <UModal
              :title="$t('rankTable.title')"
              :close="{
                size: 'sm',
                color: 'secondary',
                variant: 'solid',
              }"
              :ui="{
                content: 'max-w-5xl',
              }"
            >
              <UIcon name="crpg:help-circle" class="size-4" />
              <template #body>
                <CompetitiveRankTable
                  :rank-table="rankTable"
                  :competitive-value="gameModeCharacterStatistics.rating.competitiveValue"
                />
              </template>
            </UModal>
          </div>
        </template>

        <CompetitiveRank
          :rank-table="rankTable"
          :competitive-value="gameModeCharacterStatistics.rating.competitiveValue"
        />
      </UiSimpleTableRow>
    </div>
  </div>
</template>
