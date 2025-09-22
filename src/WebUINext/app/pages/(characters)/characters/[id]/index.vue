<script setup lang="ts">
import { useCharacter, useCharacters } from '~/composables/character/use-character'
import { CHARACTER_QUERY_KEYS } from '~/queries'
import { getCharacterStatistics, retireCharacter } from '~/services/character-service'

const userStore = useUserStore()
const { t } = useI18n()

const { character, characterId } = useCharacter()
const { refreshCharacters } = useCharacters()

const { data: characterStatistics, refresh: refreshCharacterStatistics } = useAsyncDataCustom(
  () => CHARACTER_QUERY_KEYS.statistics(characterId.value),
  () => getCharacterStatistics(characterId.value),
  {
    default: () => [],
  },
)

const [onRetireCharacter] = useAsyncCallback(async () => {
  await retireCharacter(characterId.value)
  await Promise.all([
    userStore.fetchUser(), // update experienceMultiplier
    refreshCharacters(),
    refreshCharacterStatistics(),
  ])
}, {
  successMessage: t('character.settings.retire.notify.success'),
  pageLoading: true,
})
</script>

<template>
  <div class="mx-auto max-w-3xl space-y-12">
    <UCard
      :ui="{
        body: 'space-y-6',
        footer: 'sticky bottom-0 grid grid-cols-3 gap-4 bg-bg-main/10 backdrop-blur-sm rounded-b-lg',
      }"
    >
      <CharacterOverview
        :character
        :user-experience-multiplier="userStore.user!.experienceMultiplier"
      />

      <div v-if="!character.forTournament" class="flex justify-center">
        <CharacterActionRetire
          :character
          :user-experience-multiplier="userStore.user!.experienceMultiplier"
          @retire="onRetireCharacter"
        />
      </div>

      <USeparator />

      <CharacterOverviewCompetitive
        v-if="!character.forTournament"
        :character-statistics
      />
    </UCard>
  </div>
</template>
