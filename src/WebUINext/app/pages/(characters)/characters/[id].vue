<script setup lang="ts">
import type { RouteNamedMap } from 'vue-router/auto-routes'

import { LazyCharacterCreateModal, LazyCharacterEditModal } from '#components'

import { useCharacterProvider, useCharacters } from '~/composables/character/use-character'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { activateCharacter, deactivateCharacter, deleteCharacter, updateCharacter } from '~/services/character-service'

const { t } = useI18n()
const route = useRoute('characters-id')

const userStore = useUserStore()
const { characters, refreshCharacters, activeCharacterId } = useCharacters()

if (!characters.value.find(c => c.id === Number(route.params.id))) {
  throw createError({ statusCode: 404, statusMessage: 'Character not found' })
}

const character = computed(() => characters.value.find(c => c.id === Number(route.params.id))!)
useCharacterProvider(character)

const overlay = useOverlay()

const characterCreateModal = overlay.create(LazyCharacterCreateModal)

const [onCreateNewCharacter] = useAsyncCallback(async () => {
  if (activeCharacterId.value) {
    await deactivateCharacter(activeCharacterId.value)
    await userStore.fetchUser()
  }
  characterCreateModal.open()
})

const characterEditModal = overlay.create(LazyCharacterEditModal)

const [onUpdateCharacter] = useAsyncCallback(
  async (name: string) => {
    await updateCharacter(character.value.id, { name })
    await refreshCharacters()
    characterEditModal.close()
  },
  {
    successMessage: t('character.settings.update.notify.success'),
  },
)

const [onActivateCharacter] = useAsyncCallback(
  async (characterId: number, status: boolean) => {
    status ? await activateCharacter(characterId) : await deactivateCharacter(characterId)
    await userStore.fetchUser() // update activeCharacterId
  },
  {
    successMessage: t('character.settings.update.notify.success'),
  },
)

const [onDeleteCharacter] = useAsyncCallback(
  async () => {
    characterEditModal.close()

    // TODO: deactivate char FIXME: move to backend
    if (character.value.id === activeCharacterId.value) {
      await deactivateCharacter(character.value.id)
      await userStore.fetchUser()
    }

    await deleteCharacter(character.value.id)

    return navigateTo({ name: 'characters' }, {
      external: true,
    })
  },
  {
    successMessage: t('character.settings.delete.notify.success'),
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
      v-if="character"
      class="mb-12 grid grid-cols-3 items-center gap-4"
    >
      <div class="flex items-center gap-4">
        <CharacterSelect
          :characters
          :current-character="character"
          :active-character-id="userStore.user!.activeCharacterId"
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
          :to="({ name, params: { id: character.id } })"
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
