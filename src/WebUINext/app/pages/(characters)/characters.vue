<script setup lang="ts">
import { usePageLoading } from '~/composables/app/use-page-loading'
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
    },
  ],
})

const { t } = useI18n()
const toast = useToast()

const userStore = useUserStore()
const { characters, user } = toRefs(userStore)
const { togglePageLoading } = usePageLoading()

const route = useRoute('characters-id')

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
const { execute: onUpdateCharacter, isLoading: updatingCharacter } = useAsyncCallback(
  async (name: string) => {
    if (!currentCharacter.value) {
      return
    }
    await updateCharacter(currentCharacter.value.id, { name })
    await userStore.fetchCharacters()
    toast.add({
      title: t('character.settings.update.notify.success'),
      close: false,
      color: 'success',
    })
  },
)

// activate
const { execute: onActivateCharacter, isLoading: activatingCharacter } = useAsyncCallback(
  async (id: number, status: boolean) => {
    await activateCharacter(id, status)
    await userStore.fetchUser()
    toast.add({
      title: t('character.settings.update.notify.success'),
      close: false,
      color: 'success',
    })
  },
)

// TODO: spec
const { execute: onDeleteCharacter, isLoading: deletingCharacter } = useAsyncCallback(
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

    toast.add({
      title: t('character.settings.delete.notify.success'),
      close: false,
      color: 'success',
    })

    if (userStore.characters.length === 0) {
      return navigateTo({ name: 'characters' })
    }

    return navigateTo({ name: 'characters-id', params: { id: userStore.user!.activeCharacterId || userStore.characters[0]!.id } })
  },
)

watchEffect(() => {
  togglePageLoading(updatingCharacter.value || activatingCharacter.value || deletingCharacter.value)
})
</script>

<template>
  <UContainer class="relative py-6">
    <div
      v-if="user && currentCharacter"
      data-teleport-target="character-navbar"
      class="mb-16 grid grid-cols-3 items-center justify-between gap-4"
    >
      <div class="order-1 flex items-center gap-4">
        <CharacterSelect
          :characters
          :current-character
          :active-character-id="user.activeCharacterId"
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
      :open="shownCreateCharacterGuideModal"
      @update:open="toggleCreateCharacterGuideModal(false)"
    />
  </UContainer>
</template>
