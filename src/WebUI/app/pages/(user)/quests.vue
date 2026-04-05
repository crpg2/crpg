<script setup lang="ts">
import { useAsyncState } from '@vueuse/core'
import { LazyQuestClaimDialog } from '#components'
import { groupBy } from 'es-toolkit'

import type { QuestType, UserQuest } from '~/models/quest'

import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { QUEST_TYPE } from '~/models/quest'
import { SomeRole } from '~/models/role'
import { claimQuestReward, getUserQuests, rerollQuest } from '~/services/quests-service'

definePageMeta({
  roles: SomeRole,
})

const { fetchUser } = useUser()

const typeOrder = [QUEST_TYPE.Daily, QUEST_TYPE.Weekly]

const {
  state: groupedQuests,
  isLoading,
  execute: loadQuests,
} = useAsyncState(async () =>
  Object.fromEntries(
    Object.entries(groupBy(await getUserQuests(), quest => quest.questDefinition.type))
      .sort(([aType], [bType]) => typeOrder.indexOf(aType as QuestType) - typeOrder.indexOf(bType as QuestType)),
  ), {} as Record<QuestType, UserQuest[]>, { resetOnExecute: false })

const overlay = useOverlay()
const toast = useToast()
const { t } = useI18n()

const [onClaim, claiming] = useAsyncCallback(
  async (quest: UserQuest) => {
    overlay
      .create(LazyQuestClaimDialog)
      .open({
        quest,
        onClose: async (value, characterId) => {
          if (!value || !characterId) {
            return
          }

          await claimQuestReward(quest.id, characterId)
          await Promise.all([loadQuests(), fetchUser()])

          toast.add({
            title: t('user.quests.action.claim.notify.success'),
            color: 'success',
          })
        },
      })
  },
)

const [onReroll, rerolling] = useAsyncCallback(
  async (questId: number) => {
    await rerollQuest(questId)
    await Promise.all([loadQuests(), fetchUser()])
  },
  { successMessage: t('user.quests.action.reroll.notify.success') },
)
</script>

<template>
  <UContainer class="max-w-full space-y-12 py-12">
    <UiLoading :active="isLoading" />

    <div
      v-for="(quests, type,) in groupedQuests" :key="type"
      class="space-y-6"
    >
      <UiHeading variant="h2" tag="h2" :title="`${type} Quests`" />

      <div class="grid grid-cols-3 gap-6">
        <QuestCard
          v-for="quest in quests"
          :key="quest.id"
          :quest
          @claim-reward="onClaim"
          @reroll-quest="onReroll"
        />
      </div>
    </div>

    <UiCard v-if="!isLoading && Object.keys(groupedQuests).length === 0">
      <UiResultNotFound />
    </UiCard>
  </UContainer>
</template>
