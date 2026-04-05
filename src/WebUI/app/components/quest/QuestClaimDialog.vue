<script setup lang="ts">
import type { UserQuest } from '~/models/quest'

import { useQuestDescription } from '~/composables/quest/use-quest-description'
import { useUser } from '~/composables/user/use-user'
import { getCharacters } from '~/services/character-service'

const { quest } = defineProps<{
  quest: UserQuest
}>()

const emit = defineEmits<{
  close: [boolean, characterId?: number]
}>()

const { user } = useUser()

const {
  state: characters,
  isLoading: loadingCharacters,
} = useAsyncState(() => getCharacters(), [])

const selectedCharacterId = ref<number | null>(user.value?.activeCharacterId ?? null)

const onCancel = () => {
  emit('close', false)
}

const onConfirm = async () => {
  if (selectedCharacterId.value === null) {
    return
  }

  emit('close', true, selectedCharacterId.value)
}
</script>

<template>
  <UModal
    :title="$t('user.quests.action.claim.title')"
    :ui="{
      body: 'space-y-5 text-center',
      footer: 'flex items-center justify-center gap-4',
    }"
  >
    <slot />

    <template #body>
      <i18n-t
        scope="global"
        tag="div"
        keypath="user.quests.action.claim.selectCharacter"
      >
        <template #questRewards>
          <AppCoin :value="quest.questDefinition?.rewardGold ?? 0" />&nbsp;
          <AppExperience :value="quest.questDefinition?.rewardExperience ?? 0" />
        </template>
      </i18n-t>

      <CharacterSelect
        :characters
        :current-character-id="selectedCharacterId"
        :active-character-id="user!.activeCharacterId"
        @select="(id) => selectedCharacterId = id"
      />
    </template>

    <template #footer>
      <UButton
        color="neutral"
        variant="outline"
        block
        size="xl"
        :label="$t('action.cancel')"
        @click="onCancel"
      />

      <UButton
        color="primary"
        block
        size="xl"
        :label="$t('action.confirm')"
        :disabled="!selectedCharacterId"
        @click="onConfirm()"
      />
    </template>
  </UModal>
</template>
