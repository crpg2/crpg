<script setup lang="ts">
// import { useToggle, useTransition } from '@vueuse/core'
// import {
//   experienceMultiplierByGeneration,
//   maxExperienceMultiplierForGeneration,
//   maximumLevel,
//   minimumRetirementLevel,
// } from '~root/data/constants.json'

// import type {
//   HeirloomPointByLevelAggregation,
// } from '~/services/characters-service'

// import { useCharacterRespec } from '~/composables/character/use-character-respec'
// import { useGameMode } from '~/composables/use-game-mode'
// import { usePollInterval } from '~/composables/use-poll-interval'
// import { useAsyncCallback } from '~/composables/utils/use-async-callback'
// import { GameMode } from '~/models/game-mode'
// import {
//   canRetireValidate,
//   canSetCharacterForTournamentValidate,
//   getCharacterKDARatio,
//   getCharacterStatistics,
//   getDefaultCharacterStatistics,
//   getExperienceForLevel,
//   getExperienceMultiplierBonus,
//   getHeirloomPointByLevel,
//   getHeirloomPointByLevelAggregation,
//   retireCharacter,
//   setCharacterForTournament,
//   tournamentLevelThreshold,
// } from '~/services/characters-service'
// import { checkIsRankedGameMode, gameModeToIcon } from '~/services/game-mode-service'
// import { createRankTable } from '~/services/leaderboard-service'
// import { notify } from '~/services/notification-service'
// import { t } from '~/services/translate-service'
// import { useUserStore } from '~/stores/user'
// import {
//   characterCharacteristicsKey,
//   characterKey,
//   loadCharacterStatisticsKey,
// } from '~/symbols/character'
// import { msToHours } from '~/utils/date'
// import { percentOf } from '~/utils/math'

// definePage({
//   meta: {
//     roles: ['User', 'Moderator', 'Admin'],
//   },
// })

// const userStore = useUserStore()

// const { gameModes } = useGameMode()

// const character = injectStrict(characterKey)
// const { loadCharacterCharacteristics } = injectStrict(characterCharacteristicsKey)

// const animatedCharacterExperience = useTransition(computed(() => character.value.experience))
// const currentLevelExperience = computed(() => getExperienceForLevel(character.value.level))
// const nextLevelExperience = computed(() => getExperienceForLevel(character.value.level + 1))
// const experiencePercentToNextLEvel = computed(() =>
//   percentOf(
//     character.value.experience - currentLevelExperience.value,
//     nextLevelExperience.value - currentLevelExperience.value,
//   ),
// )

// const canRetire = computed(() => canRetireValidate(character.value.level))

// const { execute: onRetireCharacter, loading: retiringCharacter } = useAsyncCallback(async () => {
//   userStore.replaceCharacter(await retireCharacter(character.value.id))
//   await Promise.all([
//     userStore.fetchUser(),
//     loadCharacterCharacteristics(0, { id: character.value.id }),
//   ])
//   notify(t('character.settings.retire.notify.success'))
// })

// const [shownRetireConfirmTooltip, toggleRetireConfirmTooltip] = useToggle()

// const canSetCharacterForTournament = computed(() =>
//   canSetCharacterForTournamentValidate(character.value),
// )

// const { execute: onSetCharacterForTournament } = useAsyncCallback(async () => {
//   userStore.replaceCharacter(await setCharacterForTournament(character.value.id))
//   await loadCharacterCharacteristics(0, { id: character.value.id })
//   notify(t('character.settings.tournament.notify.success'))
// })

// const { execute: loadCharacterStatistics, state: characterStatistics } = useAsyncState(
//   ({ id }: { id: number }) => getCharacterStatistics(id),
//   {},
//   {
//     immediate: false,
//     resetOnExecute: false,
//   },
// )

// const { subscribe, unsubscribe } = usePollInterval()

// const rankTable = computed(() => createRankTable())

// const gameMode = ref<GameMode>(GameMode.Battle)
// const isRankedGameMode = computed(() => checkIsRankedGameMode(gameMode.value))

// const gameModeCharacterStatistics = computed(
//   () => characterStatistics.value[gameMode.value] || getDefaultCharacterStatistics(),
// )

// const kdaRatio = computed(() =>
//   gameModeCharacterStatistics.value.deaths === 0
//     ? '∞'
//     : getCharacterKDARatio(gameModeCharacterStatistics.value),
// )

// const experienceMultiplierBonus = computed(() =>
//   getExperienceMultiplierBonus(userStore.user!.experienceMultiplier),
// )

// const heirloomPointByLevel = computed(() => getHeirloomPointByLevel(character.value.level))
// const retireTableData = computed(() => getHeirloomPointByLevelAggregation())

// const { loadCharacterLimitations, respecCapability, respecializingCharacter, onRespecializeCharacter } = useCharacterRespec()

// const fetchPageData = (characterId: number) =>
//   Promise.all([
//     loadCharacterStatistics(0, { id: characterId }),
//     loadCharacterLimitations(0, { id: characterId }),
//   ])

// onBeforeRouteUpdate(async (to, from) => {
//   if (to.name === from.name && 'id' in to.params) {
//     const characterId = Number(to.params.id)
//     await fetchPageData(characterId)
//     unsubscribe(loadCharacterStatisticsKey)
//     subscribe(loadCharacterStatisticsKey, () => {
//       loadCharacterStatistics(0, { id: characterId })
//     })
//   }
//   return true
// })

// await fetchPageData(character.value.id)
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-12 pb-12">
    ddd
  </div>
</template>
