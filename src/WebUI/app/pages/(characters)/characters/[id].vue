<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

import { LazyCharacterCreateModal, LazyCharacterEditModal } from '#components'

import { useCharacterProvider, useCharacters } from '~/composables/character/use-character'
import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { activateCharacter, deactivateCharacter, deleteCharacter, updateCharacter } from '~/services/character-service'

const { t } = useI18n()
const route = useRoute('characters-id')

const { user, fetchUser } = useUser()
const { characters, refreshCharacters, activeCharacterId } = useCharacters()

const character = computed(() => characters.value.find(c => c.id === Number(route.params.id))!)
if (!character.value) {
  throw createError({ statusCode: 404, statusMessage: 'Character not found' })
}
useCharacterProvider(character)

const overlay = useOverlay()

const characterCreateModal = overlay.create(LazyCharacterCreateModal)

const [onCreateNewCharacter] = useAsyncCallback(async () => {
  if (activeCharacterId.value) {
    await deactivateCharacter(activeCharacterId.value)
    await fetchUser()
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
    await fetchUser() // update activeCharacterId
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
      await fetchUser()
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

const navigationItems = computed<NavigationMenuItem[]>(() => [
  {
    label: t('character.nav.overview'),
    to: { name: 'characters-id' },
    active: route.name === 'characters-id', // hack, [id].vue conflict with [id]/index.vue
  },
  {
    label: t('character.nav.inventory'),
    to: { name: 'characters-id-inventory' },
  },
  {
    label: t('character.nav.characteristic'),
    to: { name: 'characters-id-characteristic' },
  },
  {
    label: t('character.nav.stats'),
    to: { name: 'characters-id-stats' },
  },
])
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
          :current-character-id="character.id"
          :active-character-id="user!.activeCharacterId"
          :simple="false"
          @select="(id) => navigateTo({ params: { id }, replace: true })"
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

      <UNavigationMenu
        color="primary"
        class="flex w-full justify-center gap-4"
        :items="navigationItems"
      />
    </div>

    <NuxtPage />
  </div>
</template>
