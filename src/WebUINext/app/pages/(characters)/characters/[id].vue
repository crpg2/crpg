<script setup lang="ts">
import type { RouteLocationNormalizedLoaded, RouteLocationRaw } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

import type { EquippedItemId } from '~/models/character'

import { useCharacterProvider } from '~/composables/character/use-character'
import { useCharacterCharacteristicProvider } from '~/composables/character/use-character-characteristic'
import { useCharacterItemsProvider } from '~/composables/character/use-character-items'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { updateCharacterItems as _updateCharacterItems, createEmptyCharacteristic, getCharacterCharacteristics, getCharacterItems } from '~/services/character-service'

definePageMeta({
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

const route = useRoute('characters-id')

const userStore = useUserStore()

// TODO: the character was validated in middleware, but still try to get rid of the "!"
const character = computed(() => userStore.characters.find(c => c.id === Number(route.params.id))!)

useCharacterProvider(character)

// try to provider
const {
  execute: loadCharacterItems,
  state: characterItems,
  isLoading: loadingCharacterItems,
} = useAsyncState((id: number) => getCharacterItems(id), [], { immediate: false, resetOnExecute: false })

const {
  execute: updateCharacterItems,
  isLoading: updatingCharacterItems,
} = useAsyncCallback(async (itemIds: EquippedItemId[]) => {
  characterItems.value = await _updateCharacterItems(character.value.id, itemIds)
})

useCharacterItemsProvider({
  characterItems,
  loadCharacterItems: () => loadCharacterItems(0, character.value.id),
  loadingCharacterItems,
  updateCharacterItems,
  updatingCharacterItems,
})

// TODO: FIXME:
const {
  state: characterCharacteristics,
  execute: loadCharacterCharacteristics,
} = useAsyncState(
  (id: number) => getCharacterCharacteristics(id),
  createEmptyCharacteristic(),
  { immediate: false, resetOnExecute: false },
)

useCharacterCharacteristicProvider({
  characterCharacteristics,
  loadCharacterCharacteristics: () => loadCharacterItems(0, character.value.id),
})

// const { subscribe, unsubscribe } = usePollInterval()
// const loadCharacterItemsSymbol = Symbol('loadCharacterItems')
// const loadCharactersSymbol = Symbol('fetchCharacters')
// const loadUserItemsSymbol = Symbol('fetchUserItems')

// TODO:
// onMounted(() => {
//   subscribe(loadCharactersSymbol, userStore.fetchCharacters)
//   subscribe(loadCharacterItemsSymbol, () => loadCharacterItems(0, character.value.id))
//   subscribe(loadUserItemsSymbol, userStore.fetchUserItems)
// })

// onBeforeUnmount(() => {
//   unsubscribe(loadCharactersSymbol)
//   unsubscribe(loadCharacterItemsSymbol)
//   unsubscribe(loadUserItemsSymbol)
// })

const fetchPageData = (characterId: number) => Promise.all([
  loadCharacterCharacteristics(0, characterId),
  loadCharacterItems(0, characterId),
])

onBeforeRouteUpdate((to, from) => {
  if (to.name === from.name) {
    // if character changed
    // unsubscribe(loadCharacterItemsSymbol)
    // @ts-expect-error TODO:
    const characterId = Number(to.params.id)
    fetchPageData(characterId)

    // subscribe(loadCharacterItemsSymbol, () => loadCharacterItems(0, characterId))
  }
})

fetchPageData(Number(route.params.id))

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
          :to="({ name } as RouteLocationRaw)"
        >
          <UButton
            color="neutral"
            :variant="isExactActive ? 'soft' : 'ghost'"
            size="lg"
            :label
          />
        </NuxtLink>
      </div>
    </Teleport>

    <NuxtPage />
  </div>
</template>
