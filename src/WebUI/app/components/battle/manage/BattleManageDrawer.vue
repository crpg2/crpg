<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

import type { BattleSide, BattleSideBriefing, BattleSideDetailed } from '~/models/strategus/battle'

import { useBattleMercenaryApplications, useBattleSideBriefing } from '~/composables/strategus/battle/use-battle'

const { side, battleId, onResponded } = defineProps<{
  battleId: number
  side: BattleSide
  sideInfo: BattleSideDetailed
  participantCount: number
  mercenaryApplicationId?: number
  onResponded: () => void
}>()

const emit = defineEmits<{
  close: [boolean]
}>()

const toast = useToast()
const { t } = useI18n()

const onCancel = () => {
  emit('close', false)
}

const { updateBattleBriefing, updatingBattleBriefing } = useBattleSideBriefing()

const onSaveBattleBriefing = (briefing: BattleSideBriefing) => {
  updateBattleBriefing(battleId, side, briefing)
}

const {
  mercenaryApplications,
  mercenaryPendingApplicationsCount,
  loadBattleMercenaryApplications,
  respondToBattleMercenaryApplication,
  loadingBattleMercenaryApplications,
} = useBattleMercenaryApplications()

const [onRespond, responding] = useAsyncCallback(
  async (applicationId: number, status: boolean) => {
    await respondToBattleMercenaryApplication(applicationId, status)
    await loadBattleMercenaryApplications()

    onResponded()

    toast.add({
      title: status
        ? t('strategus.battle.manage.mercenaryApplications.respond.accept.notify.success')
        : t('strategus.battle.manage.mercenaryApplications.respond.decline.notify.success'),
      close: false,
      color: 'success',
    })
  },
)

const activeTab = ref<'briefing' | 'mercenaryApplications'>('mercenaryApplications')

const items = computed<TabsItem[]>(() => [
  {
    label: t('strategus.battle.manage.briefing.title'),
    value: 'briefing',
  },
  {
    label: t('strategus.battle.manage.mercenaryApplications.title'),
    value: 'mercenaryApplications',
    ...(Boolean(mercenaryPendingApplicationsCount.value) && {
      badge: {
        variant: 'subtle',
        color: 'primary',
        size: 'md',
        label: mercenaryPendingApplicationsCount.value,
      },
    }),
  },
])
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    :ui="{
      header: 'mb-6 flex items-center justify-center gap-4',
      container: 'w-full max-w-(--ui-container) mx-auto',
      footer: 'flex flex-row justify-end',
    }"
    @close="onCancel"
  >
    <template #header>
      <div class="flex flex-1 items-center justify-center gap-4">
        <UiTextView variant="h4">
          {{ $t('strategus.battle.manage.title') }}
        </UiTextView>

        <UTabs
          v-model="activeTab"
          unmount-on-hide
          color="neutral"
          variant="pill"
          :content="false"
          :items="items"
        />
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
    </template>

    <template #body>
      <div
        class="
          mx-auto
          md:max-w-lg
        "
      >
        <BattleManageBriefingForm
          v-if="activeTab === 'briefing'"
          :briefing="sideInfo.briefing"
          :loading="updatingBattleBriefing"
          @save="onSaveBattleBriefing"
        />
      </div>

      <BattleManageMercenaryApplicationsTable
        v-if="activeTab === 'mercenaryApplications'"
        :mercenary-applications
        :mercenary-application-id
        :total-slots="sideInfo.totalParticipantSlots"
        :used-slots="participantCount"
        :loading="loadingBattleMercenaryApplications || responding"
        @respond="onRespond"
      />
    </template>
  </UDrawer>
</template>
