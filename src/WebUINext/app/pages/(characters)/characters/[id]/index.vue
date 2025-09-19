<script setup lang="ts">
import { useCharacter } from '~/composables/character/use-character'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { useAsyncStateWithPoll } from '~/composables/utils/use-async-state'
import { getCharacterStatistics, retireCharacter, setCharacterForTournament } from '~/services/character-service'
import { pollCharacterStatisticsSymbol } from '~/symbols'

const userStore = useUserStore()
const { t } = useI18n()

const { character, characterId } = useCharacter()

const {
  state: characterStatistics,
} = useAsyncStateWithPoll(
  () => getCharacterStatistics(characterId.value),
  {},
  {
    pollKey: pollCharacterStatisticsSymbol,
    pageLoading: true,
  },
)

const { respecCapability, onRespecializeCharacter } = useCharacterRespec()

const [onRetireCharacter] = useAsyncCallback(async () => {
  await retireCharacter(characterId.value)
  await Promise.all([
    userStore.fetchUser(), // update experienceMultiplier
    userStore.fetchCharacters(),
  ])
}, {
  successMessage: t('character.settings.retire.notify.success'),
  pageLoading: true,
})

const [onSetCharacterForTournament] = useAsyncCallback(async () => {
  await setCharacterForTournament(characterId.value)
  await userStore.fetchCharacters()
}, {
  successMessage: t('character.settings.tournament.notify.success'),
  pageLoading: true,
})
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-12">
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

      <USeparator />

      <CharacterOverviewCompetitive
        v-if="!character.forTournament"
        :character-statistics
      />

      <template #footer>
        <CharacterActionRespec
          :character
          :respec-capability
          @respec="onRespecializeCharacter"
        />

        <template v-if="!character.forTournament">
          <CharacterActionRetire
            :character
            :user-experience-multiplier="userStore.user!.experienceMultiplier"
            @retire="onRetireCharacter"
          />

          <CharacterActionTournament
            :character
            @tournament="onSetCharacterForTournament"
          />
        </template>
      </template>
    </UCard>
  </div>
</template>
