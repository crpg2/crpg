<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

import { LazyCharacterCreateModal, LazyCharacterEditModal } from '#components'

import { useCharacterProvider } from '~/composables/character/use-character'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { activateCharacter, deactivateCharacter, deleteCharacter, updateCharacter } from '~/services/character-service'

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

const { t } = useI18n()
const route = useRoute('characters-id')

const userStore = useUserStore()

const character = computed(() => userStore.getCurrentCharacterById(Number(route.params.id)))
useCharacterProvider(character)

const overlay = useOverlay()

const characterCreateModal = overlay.create(LazyCharacterCreateModal)
const characterEditModal = overlay.create(LazyCharacterEditModal)

const {
  execute: onCreateNewCharacter,
} = useAsyncCallback(async () => {
  if (userStore.user!.activeCharacterId) {
    await deactivateCharacter(userStore.user!.activeCharacterId)
    await userStore.fetchUser()
  }

  characterCreateModal.open()
}, {
  pageLoading: true,
})

const {
  execute: onUpdateCharacter,
} = useAsyncCallback(
  async (name: string) => {
    await updateCharacter(character.value.id, { name })
    await userStore.fetchCharacters()

    characterEditModal.close()
  },
  {
    successMessage: t('character.settings.update.notify.success'),
    pageLoading: true,
  },
)

const {
  execute: onActivateCharacter,
} = useAsyncCallback(
  async (characterId: number, status: boolean) => {
    status ? await activateCharacter(characterId) : await deactivateCharacter(characterId)
    await userStore.fetchUser() // update activeCharacterId
  },
  {
    successMessage: t('character.settings.update.notify.success'),
    pageLoading: true,
  },
)

const {
  execute: onDeleteCharacter,
} = useAsyncCallback(
  async () => {
    characterEditModal.close()

    // TODO: deactivate char FIXME: move to backend
    if (character.value.id === userStore.user!.activeCharacterId) {
      await deactivateCharacter(character.value.id)
      await userStore.fetchUser()
    }

    await deleteCharacter(character.value.id)
    await userStore.fetchCharacters()

    if (userStore.activeCharacterId) {
      return navigateTo({ name: 'characters-id', params: { id: userStore.activeCharacterId } })
    }

    return navigateTo({ name: 'characters' })
  },
  {
    successMessage: t('character.settings.delete.notify.success'),
    pageLoading: true,
  },
)

const nav = [
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
] satisfies Array<{
  name: keyof RouteNamedMap
  label: string
}>
</script>

<template>
  <div>
    <div
      class="mb-10 grid grid-cols-3 items-center gap-4"
    >
      <div class="flex items-center gap-4">
        <CharacterSelect
          :characters="userStore.characters"
          :current-character="character"
          :active-character-id="userStore.activeCharacterId"
          @activate="onActivateCharacter"
          @create="onCreateNewCharacter"
        />

        <UTooltip :text="$t('character.settings.update.title')">
          <UButton
            size="xl"
            icon="crpg:edit"
            color="neutral"
            variant="outline"
            @click="characterEditModal.open({ character, onUpdate: onUpdateCharacter, onDelete: onDeleteCharacter })"
          />
        </UTooltip>
      </div>

      <nav class="flex items-center justify-center gap-2">
        <NuxtLink
          v-for="{ name, label } in nav"
          :key="name"
          v-slot="{ isExactActive }"
          :to="({ name })"
        >
          <UButton
            color="neutral"
            variant="link"
            active-variant="soft"
            active-color="primary"
            :active="isExactActive"
            size="xl"
            :label
          />
        </NuxtLink>
      </nav>
    </div>

    <NuxtPage />
  </div>
</template>
