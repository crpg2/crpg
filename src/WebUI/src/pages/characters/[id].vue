<script setup lang="ts">
import type { CharacterCharacteristics, CharacterOverallItemsStats } from '~/models/character'

import { usePollInterval } from '~/composables/use-poll-interval'
import { useWelcome } from '~/composables/use-welcome'
import {
  computeHealthPoints,
  computeLongestWeaponLength,
  computeMountHarnessWeight,
  computeMountSpeedBase,
  computeOverallArmor,
  computeOverallAverageRepairCostByHour,
  computeOverallPrice,
  computeOverallWeight,
  createDefaultCharacteristic,
  getCharacterCharacteristics,
  getCharacterItems,
} from '~/services/characters-service'
import { useUserStore } from '~/stores/user'
import {
  characterCharacteristicsKey,
  characterHealthPointsKey,
  characterItemsKey,
  characterItemsStatsKey,
  characterKey,
} from '~/symbols/character'

const props = defineProps<{ id: string }>()

definePage({
  meta: {
    middleware: 'characterValidate',
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const userStore = useUserStore()
const character = computed(() => userStore.characters.find(c => c.id === Number(props.id))!)

const { execute: loadCharacterItems, state: characterItems } = useAsyncState(
  ({ id }: { id: number }) => getCharacterItems(id),
  [],
  {
    immediate: false,
    resetOnExecute: false,
  },
)

const { execute: loadCharacterCharacteristics, state: characterCharacteristics } = useAsyncState(
  ({ id }: { id: number }) => getCharacterCharacteristics(id),
  createDefaultCharacteristic(),
  {
    immediate: false,
    resetOnExecute: false,
  },
)

const setCharacterCharacteristics = (characteristic: CharacterCharacteristics) => {
  characterCharacteristics.value = characteristic
}

const healthPoints = computed(() =>
  computeHealthPoints(
    characterCharacteristics.value.skills.ironFlesh,
    characterCharacteristics.value.attributes.strength,
  ),
)

const itemsStats = computed((): CharacterOverallItemsStats => {
  const items = characterItems.value.map(ei => ei.userItem.item)
  return {
    averageRepairCostByHour: computeOverallAverageRepairCostByHour(items),
    longestWeaponLength: computeLongestWeaponLength(items),
    price: computeOverallPrice(items),
    weight: computeOverallWeight(items),
    mountSpeedBase: computeMountSpeedBase(items),
    mountHarnessWeight: computeMountHarnessWeight(items),
    ...computeOverallArmor(items),
  }
})

provide(characterKey, character)
provide(characterCharacteristicsKey, {
  characterCharacteristics: readonly(characterCharacteristics),
  loadCharacterCharacteristics,
  setCharacterCharacteristics,
})
provide(characterHealthPointsKey, healthPoints)
provide(characterItemsKey, {
  characterItems: readonly(characterItems),
  loadCharacterItems,
})
provide(characterItemsStatsKey, itemsStats)

const { subscribe, unsubscribe } = usePollInterval()
const loadCharacterItemsSymbol = Symbol('loadCharacterItems')
const loadCharactersSymbol = Symbol('fetchCharacters')
const loadUserItemsSymbol = Symbol('fetchUserItems')

onMounted(() => {
  subscribe(loadCharactersSymbol, userStore.fetchCharacters)
  subscribe(loadCharacterItemsSymbol, () => loadCharacterItems(0, { id: character.value.id }))
  subscribe(loadUserItemsSymbol, userStore.fetchUserItems)
})

onBeforeUnmount(() => {
  unsubscribe(loadCharactersSymbol)
  unsubscribe(loadCharacterItemsSymbol)
  unsubscribe(loadUserItemsSymbol)
})

const fetchPageData = (characterId: number) =>
  Promise.all([
    loadCharacterCharacteristics(0, { id: characterId }),
    loadCharacterItems(0, { id: characterId }),
  ])

onBeforeRouteUpdate(async (to, from) => {
  if (to.name === from.name) {
    // if character changed
    unsubscribe(loadCharacterItemsSymbol)

    // @ts-expect-error TODO:
    const characterId = Number(to.params.id)
    await fetchPageData(characterId)

    subscribe(loadCharacterItemsSymbol, () => loadCharacterItems(0, { id: characterId }))
  }

  return true
})

const { onCloseWelcomeMessage, shownWelcomeMessage, showWelcomeMessage } = useWelcome()

await fetchPageData(character.value.id)
</script>

<template>
  <div>
    <Teleport to="#character-top-navbar">
      <div class="order-2 flex items-center justify-center gap-2">
        <RouterLink
          v-slot="{ isExactActive }"
          :to="{ name: 'CharactersId', params: { id } }"
        >
          <OButton
            :variant="isExactActive ? 'transparent-active' : 'transparent'"
            size="lg"
            :label="$t('character.nav.overview')"
          />
        </RouterLink>

        <RouterLink
          v-slot="{ isActive }"
          :to="{ name: 'CharactersIdInventory', params: { id } }"
        >
          <OButton
            :variant="isActive ? 'transparent-active' : 'transparent'"
            size="lg"
            :label="$t('character.nav.inventory')"
          />
        </RouterLink>

        <RouterLink
          v-slot="{ isActive }"
          :to="{ name: 'CharactersIdCharacteristic', params: { id } }"
        >
          <OButton
            :variant="isActive ? 'transparent-active' : 'transparent'"
            size="lg"
            :label="$t('character.nav.characteristic')"
          />
        </RouterLink>

        <RouterLink
          v-slot="{ isActive }"
          :to="{ name: 'CharactersIdStats', params: { id } }"
        >
          <OButton
            :variant="isActive ? 'transparent-active' : 'transparent'"
            size="lg"
            :label="$t('character.nav.stats')"
          />
        </RouterLink>
      </div>

      <div class="order-3 flex items-center gap-2 place-self-end">
        <OButton
          v-if="userStore.isRecentUser"
          size="xl"
          rounded
          variant="transparent"
          outlined
          icon-left="help-circle"
          @click="showWelcomeMessage"
        />
        <RouterLink :to="{ name: 'Builder' }">
          <OButton
            variant="primary"
            outlined
            size="lg"
            icon-left="calculator"
            :label="$t(`nav.main.Builder`)"
          />
        </RouterLink>
      </div>
    </Teleport>

    <RouterView />

    <Welcome
      v-if="shownWelcomeMessage"
      @close="onCloseWelcomeMessage"
    />
  </div>
</template>
