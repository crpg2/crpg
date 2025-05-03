<script setup lang="ts">
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import {
  activateCharacter,
  deleteCharacter,
  updateCharacter,
} from '~/services/character-service'

definePageMeta({
  roles: SomeRole,
  middleware: [
    /**
     * @description
     * load characters
     */
    async () => {
      const userStore = useUserStore()
      if (userStore.characters.length === 0) {
        await userStore.fetchCharacters()
      }
      return true
    },
  ],
})

const { t } = useI18n()
const { $notify } = useNuxtApp()

const userStore = useUserStore()
const { characters, user } = toRefs(userStore)

const route = useRoute('characters-id')
const router = useRouter()

const currentCharacterId = computed(() =>
  route.params.id ? Number(route.params.id) : null,
)

const currentCharacter = computed(() => characters.value.find(char => char.id === currentCharacterId.value))

// create
const [shownCreateCharacterGuideModal, toggleCreateCharacterGuideModal] = useToggle()
const { execute: onCreateNewCharacter } = useAsyncCallback(async () => {
  if (user.value!.activeCharacterId) {
    await activateCharacter(user.value!.activeCharacterId, false)
    await userStore.fetchUser()
  }
  toggleCreateCharacterGuideModal(true)
})

// update name
const { execute: onUpdateCharacter } = useAsyncCallback(
  async (name: string) => {
    if (!currentCharacter.value) {
      return
    }
    await updateCharacter(currentCharacter.value.id, { name })
    await userStore.fetchUser()
    $notify(t('character.settings.update.notify.success'))
  },
)

// activate
const { execute: onActivateCharacter } = useAsyncCallback(
  async (id: number, status: boolean) => {
    await activateCharacter(id, status)
    await userStore.fetchUser()
    $notify(t('character.settings.update.notify.success'))
  },
)

const { execute: onDeleteCharacter } = useAsyncCallback(
  async () => {
    if (!currentCharacter.value) {
      return
    }

    if (currentCharacter.value.id === userStore.user!.activeCharacterId) {
      await activateCharacter(currentCharacter.value.id, false)
      await userStore.fetchUser()
    }

    await deleteCharacter(currentCharacter.value.id)
    await userStore.fetchCharacters()

    $notify(t('character.settings.delete.notify.success'))

    if (userStore.characters.length === 0) {
      return navigateTo({ name: 'characters' })
    }

    return navigateTo({ name: 'characters-id', params: { id: userStore.user!.activeCharacterId || userStore.characters[0]!.id } })
  },
)
</script>

<template>
  <div class="container relative py-6">
    <div
      v-if="currentCharacter"
      data-teleport-target="character-navbar"
      class="mb-16 grid grid-cols-3 items-center justify-between gap-4"
    >
      <div class="order-1 flex items-center gap-4">
        <CharacterSelect
          :characters
          :current-character
          :active-character-id="user!.activeCharacterId"
          @activate="onActivateCharacter"
          @create="onCreateNewCharacter"
        />

        <CharacterEditModal
          :character="currentCharacter"
          @update="onUpdateCharacter"
          @delete="onDeleteCharacter"
        />
      </div>
    </div>

    <NuxtPage />

    <CharacterCreateModal
      :shown="shownCreateCharacterGuideModal"
      @apply-hide="toggleCreateCharacterGuideModal(false)"
    />
  </div>
</template>
