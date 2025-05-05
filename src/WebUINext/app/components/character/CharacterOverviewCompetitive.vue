<script setup lang="ts">
import type { CharacterStatistics } from '~/models/character'

import { GameMode } from '~/models/game-mode'
import { getCharacterKDARatio, getDefaultCharacterStatistics } from '~/services/character-service'
import { checkIsRankedGameMode, gameModeToIcon } from '~/services/game-mode-service'
import { msToHours } from '~/utils/date'

const { characterStatistics } = defineProps<{
  characterStatistics: Partial<Record<GameMode, CharacterStatistics>>
}>()

const gameMode = ref<GameMode>(GameMode.Battle)
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
  <div>
    <div class="flex justify-center">
      <OTabs
        v-model="gameMode"
        content-class="hidden"
        multiline
      >
        <OTabItem
          v-for="gm in Object.values(GameMode)"
          :key="gm"
          :label="$t(`game-mode.${gm}`, 0)"
          :icon="gameModeToIcon[gm]"
          :value="gm"
        />
      </OTabs>
    </div>

    <div class="grid grid-cols-2 gap-2 text-2xs">
      <UiSimpleTableRow
        v-if="isRankedGameMode"
        :label="$t('character.statistics.rank.title')"
      >
        <UiTooltip
          :title="$t('character.statistics.rank.tooltip.title')"
          :description="$t('character.statistics.rank.tooltip.desc')"
        >
          <CompetitiveRank
            :rank-table="rankTable"
            :competitive-value="gameModeCharacterStatistics.rating.competitiveValue"
          />
        </UiTooltip>
        <UiModal closable>
          <UiTag
            icon="help-circle"
            rounded
            size="lg"
            variant="primary"
          />
          <template #popper>
            <CompetitiveRankTable
              :rank-table="rankTable"
              :competitive-value="gameModeCharacterStatistics.rating.competitiveValue"
            />
          </template>
        </UiModal>
      </UiSimpleTableRow>

      <UiSimpleTableRow
        :label="$t('character.statistics.kda.title')"
        :value="
          $t('character.format.kda', {
            kills: gameModeCharacterStatistics.kills,
            deaths: gameModeCharacterStatistics.deaths,
            assists: gameModeCharacterStatistics.assists,
            ratio: kdaRatio,
          })
        "
        :tooltip="{
          title: $t('character.statistics.kda.tooltip.title'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.statistics.playTime.title')"
        :value="
          $t('dateTimeFormat.hh', { hours: msToHours(gameModeCharacterStatistics.playTime) })
        "
      />
    </div>
  </div>
</template>
