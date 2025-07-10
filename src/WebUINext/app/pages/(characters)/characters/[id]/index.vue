<script setup lang="ts">
import { usePageLoading } from '~/composables/app/use-page-loading'
import { useCharacter } from '~/composables/character/use-character'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { getCharacterStatistics, retireCharacter, setCharacterForTournament } from '~/services/character-service'

const userStore = useUserStore()
const route = useRoute('characters-id')
const { t } = useI18n()
const toast = useToast()

const { character } = useCharacter()

const {
  state: characterStatistics,
  execute: loadCharacterStatistics,
} = useAsyncState(
  (id: number) => getCharacterStatistics(id),
  {},
  { immediate: false, resetOnExecute: false },
)

const {
  execute: onRetireCharacter,
  isLoading: retiringCharacter,
} = useAsyncCallback(async () => {
  await retireCharacter(character.value.id)

  await Promise.all([
    userStore.fetchUser(), // update user
    userStore.fetchCharacters(), // update char
    // loadCharacterCharacteristics(0, { id: character.value.id }),
  ])

  toast.add({
    title: t('character.settings.retire.notify.success'),
    close: false,
    color: 'success',
  })
})

const {
  execute: onSetCharacterForTournament,
  isLoading: settingCharacterForTournament,
} = useAsyncCallback(async () => {
  await setCharacterForTournament(character.value.id)

  await Promise.all([
    userStore.fetchCharacters(), // update char
    // loadCharacterCharacteristics(0, { id: character.value.id }),
  ])

  toast.add({
    title: t('character.settings.tournament.notify.success'),
    close: false,
    color: 'success',
  })
})

const { subscribe, unsubscribe } = usePollInterval()

const {
  loadCharacterLimitations,
  respecCapability,
  respecializingCharacter,
  onRespecializeCharacter,
} = useCharacterRespec()

const fetchPageData = (characterId: number) => Promise.all([
  loadCharacterStatistics(0, characterId),
  loadCharacterLimitations(0, characterId),
])

const loadCharacterStatisticsKey = Symbol('loadCharacterStatistics')

onBeforeRouteUpdate((to, from) => {
  if (to.name === from.name && 'id' in to.params) {
    const characterId = Number(to.params.id)
    unsubscribe(loadCharacterStatisticsKey)
    subscribe(loadCharacterStatisticsKey, () => {
      loadCharacterStatistics(0, characterId)
    })
    fetchPageData(characterId)
  }
})

fetchPageData(Number(route.params.id))

const { togglePageLoading } = usePageLoading()

watchEffect(() => {
  togglePageLoading(respecializingCharacter.value || retiringCharacter.value || settingCharacterForTournament.value)
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
          @respec="() => onRespecializeCharacter(character.id)"
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
