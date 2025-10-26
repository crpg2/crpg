<script setup lang="ts">
import type { Battle, BattleSide } from '~/models/strategus/battle'

import { BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType } from '~/services/strategus/battle-service'

const { battle } = defineProps<{
  battle: Battle
  canApply: Record<BattleSide, {
    disabled: {
      reason: string
    } | null
  } | null>
  canManage: Record<BattleSide, boolean>
}>()

defineEmits<{
  openMercenaryApplication: [BattleSide]
  openManage: [BattleSide]
}>()

const sides = computed(() => [
  {
    side: BATTLE_SIDE.Attacker,
    sideInfo: battle.attacker,
  },
  {
    side: BATTLE_SIDE.Defender,
    sideInfo: battle.defender,
  },
])
</script>

<template>
  <div class="grid grid-cols-[1fr_auto_1fr] gap-6">
    <template v-for="({ side, sideInfo }, idx) in sides" :key="side">
      <BattleSideView
        class="overflow-hidden"
        :side
        :side-info
      >
        <template #topbar-prepend>
          <BattleMercenaryApplicationStatusBadge
            v-if="sideInfo.mercenaryApplication"
            :application-status="sideInfo.mercenaryApplication.status"
            @click="$emit('openMercenaryApplication', side)"
          />
        </template>

        <template #append>
          <UTooltip
            v-if="canApply[side]"
            :disabled="!Boolean(canApply[side].disabled)"
            :text="canApply[side].disabled?.reason"
          >
            <UButton
              label="Apply as mercenary"
              icon="crpg:mercenary"
              variant="subtle"
              :disabled="Boolean(canApply[side].disabled)"
              class="cursor-pointer"
              @click="$emit('openMercenaryApplication', side)"
            />
          </UTooltip>

          <UButton
            v-if="canManage[side]"
            label="Manage battle"
            icon="crpg:settings"
            variant="subtle"
            @click="$emit('openManage', side)"
          />
        </template>
      </BattleSideView>

      <UTooltip
        v-if="idx === 0"
        :text="battle.type"
        :content="{ side: 'top' }"
      >
        <USeparator
          orientation="vertical"
          class="h-28 self-center"
          size="sm"
          :icon="`crpg:${battleIconByType[battle.type]}`"
          :ui="{ icon: 'size-7' }"
        />
      </UTooltip>
    </template>
  </div>
</template>
