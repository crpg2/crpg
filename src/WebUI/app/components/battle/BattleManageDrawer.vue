<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

import type { BattleMercenaryApplication, BattleSide, BattleSideBriefing, BattleSideDetailed } from '~/models/strategus/battle'

import { useBattleMercenaryApplications, useBattleSideBriefing } from '~/composables/strategus/battle/use-battle'

const { side, sideInfo, userId, battleId } = defineProps<{
  side: BattleSide
  sideInfo: BattleSideDetailed
  userId: number
  battleId: number
}>()

const emit = defineEmits<{
  close: [boolean]
}>()

const toast = useToast()

const onCancel = () => {
  emit('close', false)
}

const { updateBattleBriefing, updatingBattleBriefing } = useBattleSideBriefing()

const onSaveBattleBriefing = async (briefing: BattleSideBriefing) => {
  await updateBattleBriefing(battleId, side, briefing)
}

const {
  mercenaryApplications,
  mercenaryApplicationsCount,
  loadBattleMercenaryApplications,
  respondToBattleMercenaryApplication,
} = useBattleMercenaryApplications()

const [onRespond, responding] = useAsyncCallback(
  async (applicationId: number, status: boolean) => {
    await respondToBattleMercenaryApplication(applicationId, status)
    await loadBattleMercenaryApplications()
    toast.add({
      title: status
        ? 'TODO:'
        : 'TODO:',
      close: false,
      color: 'success',
    })
  },
)

const activeTab = ref<'briefing' | 'mercenaryApplications'>('mercenaryApplications')

const items = computed<TabsItem[]>(() => [
  {
    label: 'Briefing',
    value: 'briefing',
  },
  {
    label: 'Mercenary applications',
    value: 'mercenaryApplications',
    badge: {
      variant: 'subtle',
      color: 'primary',
      size: 'md',
      square: true,
      label: mercenaryApplicationsCount.value,
    },
  },
])
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    :ui="{
      header: 'mb-6 flex items-center justify-between gap-4',
      container: 'w-full max-w-(--ui-container) mx-auto',
      footer: 'flex flex-row justify-end' }"
  >
    <template #header>
      <div class="flex items-center gap-4">
        <UiTextView variant="h4">
          Manage Battle
        </UiTextView>

        <UTabs
          v-model="activeTab"
          color="neutral"
          variant="link"
          :content="false"
          :items="items"
        />
      </div>

      <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
    </template>

    <template #body>
      <div class="md:max-w-lg">
        <BattleSideBriefingForm
          v-if="activeTab === 'briefing'"
          :briefing="sideInfo.briefing"
          @save="onSaveBattleBriefing"
        />
      </div>

      <BattleSideMercenaryApplicationsTable
        v-if="activeTab === 'mercenaryApplications'"
        :mercenary-applications
        @respond="onRespond"
      />
    </template>

    <template #footer />
  </UDrawer>
</template>
