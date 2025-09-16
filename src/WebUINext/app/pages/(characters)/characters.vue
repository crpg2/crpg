<script setup lang="ts">
import { LazyCharacterCreateModal, LazyCharacterEditModal } from '#components'

import { usePageLoading } from '~/composables/app/use-page-loading'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { SomeRole } from '~/models/role'
import {
  activateCharacter,
  deactivateCharacter,
  deleteCharacter,
  updateCharacter,
} from '~/services/character-service'
import { pollUserCharactersSymbol } from '~/symbols'

definePageMeta({
  roles: SomeRole,
  middleware: [
    /**
     * @description
     * load characters
     */
    async () => {
      const userStore = useUserStore()
      if (!userStore.characters.length) {
        await userStore.fetchCharacters()
      }
    },
  ],
})

const { t } = useI18n()
const toast = useToast()

const userStore = useUserStore()
const { characters, user } = toRefs(userStore)

usePollInterval({
  key: pollUserCharactersSymbol,
  fn: userStore.fetchCharacters,
})

const route = useRoute('characters-id')
const overlay = useOverlay()

const currentCharacterId = computed(() =>
  route.params.id ? Number(route.params.id) : null,
)

const currentCharacter = computed(() => characters.value.find(char => char.id === currentCharacterId.value) ?? null)

const characterCreateModal = overlay.create(LazyCharacterCreateModal)

const { execute: onCreateNewCharacter, isLoading: creatingNewCharacter } = useAsyncCallback(async () => {
  if (!user.value) {
    return
  }

  if (user.value.activeCharacterId) {
    await deactivateCharacter(user.value.activeCharacterId)
    await userStore.fetchUser()
  }

  characterCreateModal.open()
})

const {
  execute: onUpdateCharacter,
  isLoading: updatingCharacter,
} = useAsyncCallback(
  async (name: string) => {
    if (!currentCharacter.value) {
      return
    }

    await updateCharacter(currentCharacter.value.id, { name })
    await userStore.fetchCharacters()

    // eslint-disable-next-line ts/no-use-before-define
    characterEditModal.close()

    toast.add({
      title: t('character.settings.update.notify.success'),
      close: false,
      color: 'success',
    })
  },
)

const {
  execute: onActivateCharacter,
  isLoading: activatingCharacter,
} = useAsyncCallback(
  async (id: number, status: boolean) => {
    status ? await activateCharacter(id) : await deactivateCharacter(id)
    await userStore.fetchUser()
    toast.add({
      title: t('character.settings.update.notify.success'),
      close: false,
      color: 'success',
    })
  },
)

const {
  execute: onDeleteCharacter,
  isLoading: deletingCharacter,
} = useAsyncCallback(
  async () => {
    if (!user.value || !currentCharacter.value) {
      return
    }

    // TODO: deactivate char FIXME: move to backend
    if (currentCharacter.value.id === user.value.activeCharacterId) {
      await deactivateCharacter(currentCharacter.value.id)
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

    return navigateTo({ name: 'characters-id', params: { id: user.value.activeCharacterId || userStore.characters[0]!.id } })
  },
)

usePageLoading({
  watch: [creatingNewCharacter, updatingCharacter, activatingCharacter, deletingCharacter],
})

const characterEditModal = overlay.create(LazyCharacterEditModal, {
  props: {
    character: currentCharacter.value!,
    onUpdate: onUpdateCharacter,
    onDelete: onDeleteCharacter,
  },
})
</script>

<template>
  <UContainer class="relative py-6">
    <div
      data-teleport-target="character-navbar"
      class="mb-16 grid grid-cols-3 items-center justify-between gap-4"
    >
      <div v-if="user && currentCharacter" class="order-1 flex items-center gap-4">
        <CharacterSelect
          :characters
          :current-character
          :active-character-id="user.activeCharacterId"
          @activate="onActivateCharacter"
          @create="onCreateNewCharacter"
        />

        <UTooltip
          :text="$t('character.settings.update.title')"
        >
          <UButton
            size="xl"
            icon="crpg:edit"
            color="neutral"
            variant="outline"
            @click="() => characterEditModal.open()"
          />
        </UTooltip>
      </div>
    </div>

    <NuxtPage />
  </UContainer>
</template>
