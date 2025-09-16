<script setup lang="ts">
import type { RouteLocationNormalizedLoaded, RouteLocationRaw } from 'vue-router'

import { useCharacterProvider } from '~/composables/character/use-character'
import { useCharacterCharacteristicProvider } from '~/composables/character/use-character-characteristic'
import { useCharacterItemsProvider } from '~/composables/character/use-character-items'
import { useCharacterLimitationsProvider } from '~/composables/character/use-character-limitations'

definePageMeta({
  middleware: [
    /**
     * @description Validate character
     */
    (to) => {
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

const { characterId } = useCharacterProvider(() => Number(route.params.id))
useCharacterItemsProvider(characterId)
useCharacterCharacteristicProvider(characterId)
useCharacterLimitationsProvider(characterId)

const { t } = useI18n()

const links = [
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
          :to="({ name, params: { id: characterId } } as RouteLocationRaw)"
        >
          <UButton
            color="neutral"
            variant="ghost"
            active-variant="soft"
            :active="isExactActive"
            size="lg"
            :label
          />
        </NuxtLink>
      </div>
    </Teleport>

    <NuxtPage />
  </div>
</template>
