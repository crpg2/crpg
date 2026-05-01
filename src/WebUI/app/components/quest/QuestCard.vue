<script setup lang="ts">
import { questRerollDailyQuestPrice } from '~root/data/constants.json'

import type { UserQuest } from '~/models/quest'

import { useQuestTitle } from '~/composables/use-quest-title'
import { QUEST_TYPE } from '~/models/quest'

const { quest } = defineProps<{
  quest: UserQuest
}>()

defineEmits<{
  claimReward: [quest: UserQuest]
  rerollQuest: [questId: number]
}>()

function progressPercent(quest: UserQuest): number {
  const required = quest.questDefinition?.requiredValue ?? 1
  return Math.min(100, Math.round((quest.currentValue / required) * 100))
}

const isCompleted = computed(() => quest.currentValue >= (quest.questDefinition?.requiredValue ?? 0))
const isExpired = computed(() => new Date(quest.expiresAt) < new Date())

const timeRemaining = computed(() => parseTimestamp((new Date(quest.expiresAt).getTime() - Date.now())))

const canReroll = computed(() => !isExpired.value && !quest.isRewardClaimed && quest.questDefinition.type === QUEST_TYPE.Daily)
const canClaim = computed(() => !isExpired.value && !quest.isRewardClaimed && isCompleted.value)

const RenderQuestTitle = useQuestTitle(quest.questDefinition)
</script>

<template>
  <UCard
    :variant="quest.isRewardClaimed ? 'soft' : 'subtle'"
    :class="{ 'opacity-66': quest.isRewardClaimed }"
    :ui="{
      root: 'flex flex-col',
      body: 'space-y-4.5 flex-1',
      footer: 'flex justify-end gap-2',
    }"
  >
    <template #header>
      <div class="flex items-start justify-between gap-2">
        <div>
          <UiTextView variant="h5" margin-bottom>
            <RenderQuestTitle />
          </UiTextView>
        </div>

        <div class="flex shrink-0 items-center gap-2">
          <UTooltip v-if="!isExpired && !quest.isRewardClaimed" :text="$d(quest.expiresAt, 'short')">
            <UBadge
              variant="subtle"
              icon="i-lucide-clock"
            >
              {{ quest.questDefinition.type === QUEST_TYPE.Daily
                ? $t('dateTimeFormat.hh:mm', { ...timeRemaining })
                : $t('dateTimeFormat.dd:hh:mm', { ...timeRemaining })
              }}
            </UBadge>
          </UTooltip>

          <UBadge
            v-if="quest.isRewardClaimed"
            color="success"
            :label="$t('user.quests.status.claimed')"
          />
          <UBadge
            v-else-if="isExpired"
            color="warning"
            :label="$t('user.quests.status.expired')"
          />
        </div>
      </div>
    </template>

    <UProgress
      :model-value="progressPercent(quest)"
      status
      size="lg"
      class="flex-1"
      :ui="{
        status: 'text-highlighted',
      }"
    >
      <template #status="{ percent }">
        {{ percent }}%&nbsp;&nbsp;·&nbsp;&nbsp;{{ $n(quest.currentValue) }}/{{ $n(quest.questDefinition?.requiredValue ?? 0) }}
      </template>
    </UProgress>

    <div class="flex flex-wrap items-center justify-between gap-4">
      <div class="flex items-center gap-3.5">
        <AppCoin :value="quest.questDefinition.rewardGold" />
        <AppExperience :value="quest.questDefinition.rewardExperience" />
      </div>
    </div>

    <template v-if="canReroll || canClaim" #footer>
      <AppConfirmActionPopover
        v-if="canReroll"
        :confirm-label="$t('user.quests.action.reroll.label')"
        @confirm="$emit('rerollQuest', quest.id)"
      >
        <UButton
          color="neutral"
          variant="soft"
          icon="i-lucide-dices"
          size="lg"
          :label="$t('user.quests.action.reroll.label')"
        />
        <template #title>
          <UiTextView variant="h5">
            <i18n-t
              scope="global"
              keypath="user.quests.action.reroll.confirmation"
              tag="div"
            >
              <template #goldCost>
                <AppCoin :value="questRerollDailyQuestPrice" />
              </template>
            </i18n-t>
          </UiTextView>
        </template>
      </AppConfirmActionPopover>

      <UButton
        v-if="canClaim"
        color="primary"
        size="lg"
        variant="subtle"
        icon="crpg:chest"
        :label="$t('user.quests.action.claim.label')"
        @click="$emit('claimReward', quest)"
      />
    </template>
  </UCard>
</template>
