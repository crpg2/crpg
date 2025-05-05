<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

// import type { CharacterCharacteristics, CharacterOverallItemsStats } from '~/models/character'
// import { usePollInterval } from '~/composables/use-poll-interval'
// import { useWelcome } from '~/composables/use-welcome'
// import {
//   computeHealthPoints,
//   computeLongestWeaponLength,
//   computeOverallArmor,
//   computeOverallAverageRepairCostByHour,
//   computeOverallPrice,
//   computeOverallWeight,
//   createDefaultCharacteristic,
//   getCharacterCharacteristics,
//   getCharacterItems,
// } from '~/services/characters-service'
// import { useUserStore } from '~/stores/user'
import {
  // characterCharacteristicsKey,
  // characterHealthPointsKey,
  // characterItemsKey,
  // characterItemsStatsKey,
  characterKey,
} from '~/symbols/character'

const props = defineProps<{ id: string }>()

definePageMeta({
  props: true,
  middleware: [
    /**
     * @description Validate character
     */
    async (to) => {
      const userStore = useUserStore()

      if (!userStore.validateCharacter(Number((to as RouteLocationNormalizedLoaded<'characters-id'>).params.id))) {
        return navigateTo({
          name: 'characters',
        })
      }
    },
  ],
})

const characterId = computed(() => Number(props.id))

const userStore = useUserStore()

// TODO: the character was validated in middleware, but still try to get rid of the "!"
const character = computed(() => userStore.characters.find(c => c.id === characterId.value)!)

provide(characterKey, character) // pass the character object further down the context, to the child pages

// const { execute: loadCharacterItems, state: characterItems } = useAsyncState(
//   ({ id }: { id: number }) => getCharacterItems(id),
//   [],
//   {
//     immediate: false,
//     resetOnExecute: false,
//   },
// )

// const { execute: loadCharacterCharacteristics, state: characterCharacteristics } = useAsyncState(
//   ({ id }: { id: number }) => getCharacterCharacteristics(id),
//   createDefaultCharacteristic(),
//   {
//     immediate: false,
//     resetOnExecute: false,
//   },
// )

// const setCharacterCharacteristics = (characteristic: CharacterCharacteristics) => {
//   characterCharacteristics.value = characteristic
// }

// const healthPoints = computed(() =>
//   computeHealthPoints(
//     characterCharacteristics.value.skills.ironFlesh,
//     characterCharacteristics.value.attributes.strength,
//   ),
// )

// const itemsStats = computed((): CharacterOverallItemsStats => {
//   const items = characterItems.value.map(ei => ei.userItem.item)
//   return {
//     averageRepairCostByHour: computeOverallAverageRepairCostByHour(items),
//     longestWeaponLength: computeLongestWeaponLength(items),
//     price: computeOverallPrice(items),
//     weight: computeOverallWeight(items),
//     ...computeOverallArmor(items),
//   }
// })

// provide(characterCharacteristicsKey, {
//   characterCharacteristics: readonly(characterCharacteristics),
//   loadCharacterCharacteristics,
//   setCharacterCharacteristics,
// })
// provide(characterHealthPointsKey, healthPoints)
// provide(characterItemsKey, {
//   characterItems: readonly(characterItems),
//   loadCharacterItems,
// })
// provide(characterItemsStatsKey, itemsStats)

// const { subscribe, unsubscribe } = usePollInterval()
// const loadCharacterItemsSymbol = Symbol('loadCharacterItems')
// const loadCharactersSymbol = Symbol('fetchCharacters')
// const loadUserItemsSymbol = Symbol('fetchUserItems')

// onMounted(() => {
//   subscribe(loadCharactersSymbol, userStore.fetchCharacters)
//   subscribe(loadCharacterItemsSymbol, () => loadCharacterItems(0, { id: character.value.id }))
//   subscribe(loadUserItemsSymbol, userStore.fetchUserItems)
// })

// onBeforeUnmount(() => {
//   unsubscribe(loadCharactersSymbol)
//   unsubscribe(loadCharacterItemsSymbol)
//   unsubscribe(loadUserItemsSymbol)
// })

// const fetchPageData = (characterId: number) =>
//   Promise.all([
//     loadCharacterCharacteristics(0, { id: characterId }),
//     loadCharacterItems(0, { id: characterId }),
//   ])

// onBeforeRouteUpdate(async (to, from) => {
//   if (to.name === from.name) {
//     // if character changed
//     unsubscribe(loadCharacterItemsSymbol)

//     // @ts-expect-error TODO:
//     const characterId = Number(to.params.id)
//     await fetchPageData(characterId)

//     subscribe(loadCharacterItemsSymbol, () => loadCharacterItems(0, { id: characterId }))
//   }

//   return true
// })

// await fetchPageData(character.value.id)

const { onCloseWelcomeMessage, shownWelcomeMessage, showWelcomeMessage } = useWelcome()

const { t } = useI18n()

const links: { name: keyof RouteNamedMap, label: string }[] = [
  {
    name: 'characters-id',
    label: t('character.nav.overview'),
  },
  {
    name: 'characters-id-inventory',
    label: t('character.nav.inventory'),
  },
  {
    name: 'characters-id-characteristic',
    label: t('character.nav.characteristic'),
  },
  {
    name: 'characters-id-stats',
    label: t('character.nav.stats'),
  },
]
</script>

<template>
  <div>
    <Teleport to="[data-teleport-target='character-navbar']" defer>
      <div class="order-2 flex items-center justify-center gap-2">
        <NuxtLink
          v-for="{ name, label } in links"
          :key="name"
          v-slot="{ isExactActive }"
          :to="{ name }"
        >
          <OButton
            :variant="isExactActive ? 'transparent-active' : 'secondary'"
            size="lg"
            :label
          />
        </NuxtLink>
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
        <!-- TODO: FIXME: to global nav -->
        <!-- <RouterLink :to="{ name: 'Builder' }">
          <OButton
            variant="primary"
            outlined
            size="lg"
            icon-left="calculator"
            :label="$t(`nav.main.Builder`)"
          />
        </RouterLink> -->
      </div>
    </Teleport>

    <NuxtPage />

    <AppWelcome
      v-if="shownWelcomeMessage"
      @close="onCloseWelcomeMessage"
    />
  </div>
</template>
