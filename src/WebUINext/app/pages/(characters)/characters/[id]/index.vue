<script setup lang="ts">
import { useCharacter } from '~/composables/character/use-character'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { getCharacterStatistics, retireCharacter, setCharacterForTournament } from '~/services/character-service'

const userStore = useUserStore()
const { t } = useI18n()

const { character, characterId } = useCharacter()

const CHARACTER_QUERY_KEYS = {
  root: ['character'] as const,
  byId: (id: number) => [...CHARACTER_QUERY_KEYS.root, id] as const,
  statistics: (id: number) => [...CHARACTER_QUERY_KEYS.byId(id), { statistics: true }] as const,
}

const { data: characterStatistics } = useAsyncDataCustom(
  () => CHARACTER_QUERY_KEYS.statistics(characterId.value),
  () => getCharacterStatistics(characterId.value),
  {
    default: () => [],
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
