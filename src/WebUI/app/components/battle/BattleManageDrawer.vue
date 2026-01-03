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

const activeTab = ref<'briefing' | 'mercenaryApplications'>('briefing')

const items = ref<TabsItem[]>([
  {
    label: 'Briefing',
    value: 'briefing',
  },
  {
    label: 'Mercenary applications',
    value: 'mercenaryApplications',
    badge: mercenaryApplicationsCount.value,
  },
])
</script>

<template>
  <UDrawer
    direction="right"
    :handle="false"
    :ui="{ container: 'max-w-3xl min-w-2xl', footer: 'flex flex-row justify-end' }"
  >
    <template #header>
      <div class="mb-4 flex items-center justify-between gap-4">
        <h2 class="font-semibold text-highlighted">
          Manage Battle
        </h2>

        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>

      <UTabs
        v-model="activeTab"
        color="neutral"
        variant="link"
        :content="false"
        :items="items"
        class="w-full"
      />
    </template>

    <template #body>
      <BattleSideBriefingForm
        v-if="activeTab === 'briefing'"
        :briefing="sideInfo.briefing"
        @save="onSaveBattleBriefing"
      />

      <BattleSideMercenaryApplications
        v-if="activeTab === 'mercenaryApplications'"
        :mercenary-applications
        @respond="onRespond"
      />
    </template>

    <template #footer />
  </UDrawer>
</template>
