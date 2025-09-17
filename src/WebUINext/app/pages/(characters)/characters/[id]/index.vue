<script setup lang="ts">
import { useCharacter } from '~/composables/character/use-character'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { useCharacterRetire } from '~/composables/character/use-character-retire'
import { useCharacterTournament } from '~/composables/character/use-character-tournament'
import { useAsyncStateWithPoll } from '~/composables/utils/use-async-state'
import { getCharacterStatistics } from '~/services/character-service'
import { pollCharacterStatisticsSymbol } from '~/symbols'

const userStore = useUserStore()

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
const { onRetireCharacter } = useCharacterRetire()
const { onSetCharacterForTournament } = useCharacterTournament()
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
