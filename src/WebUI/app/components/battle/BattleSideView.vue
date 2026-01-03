<script setup lang="ts">
import type { BattleSide, BattleSideDetailed } from '~/models/strategus/battle'

import { BATTLE_SIDE } from '~/models/strategus/battle'
import { settlementIconByType } from '~/services/strategus/settlement-service'

const { sideInfo, userId } = defineProps<{
  side: BattleSide
  sideInfo: BattleSideDetailed
  userId: number
}>()

defineEmits<{
  applyToJoin: []
  openManage: []
}>()

// TODO:
const canApply = computed(() => sideInfo.fighter.party?.user.id !== userId)
</script>

<template>
  <div
    class="flex flex-1 flex-col gap-y-3.5"
    :class="side === BATTLE_SIDE.Attacker ? 'items-end text-right' : 'items-start text-left'"
  >
    <div
      class="flex items-center gap-2"
      :class="side === BATTLE_SIDE.Attacker
        ? 'flex-row justify-end' : 'flex-row-reverse justify-end'"
    >
      <BattleApplicationStatus
        v-if="canApply"
        :application-status="sideInfo.applicationStatus"
        @apply-to-join="$emit('applyToJoin')"
      />

      <UiTextView variant="caption-sm">
        {{ $t(`strategus.battle.side.${side}`) }}
      </UiTextView>

      <UBadge icon="crpg:member" :label="$n(sideInfo.totalTroops)" variant="subtle" />
    </div>

    <template v-if="sideInfo.fighter.party">
      <UiDataMedia
        v-if="sideInfo.fighter.party.user.clanMembership?.clan"
        :label="sideInfo.fighter.party.user.clanMembership.clan.name"
        size="xl"
        :layout="side === BATTLE_SIDE.Attacker ? 'reverse' : 'normal'"
        :style="{ color: sideInfo.fighter.party.user.clanMembership.clan.primaryColor }"
      >
        <template #icon="{ classes: clanTagIconClasses }">
          <ClanTagIcon
            :color="sideInfo.fighter.party.user.clanMembership.clan.primaryColor"
            :class="clanTagIconClasses()"
          />
        </template>
      </UiDataMedia>

      <UserMedia :user="sideInfo.fighter.party.user" />
    </template>

    <template v-else-if="sideInfo.fighter.settlement">
      <template v-if="sideInfo.fighter.settlement.owner?.clanMembership?.clan">
        <UiDataMedia
          :label="sideInfo.fighter.settlement.owner.clanMembership.clan.name"
          size="xl"
        >
          <template #icon="{ classes: clanTagIconClasses }">
            <ClanTagIcon
              :color="sideInfo.fighter.settlement.owner.clanMembership.clan.primaryColor"
              :class="clanTagIconClasses()"
            />
          </template>
        </UiDataMedia>
      </template>

      <div class="flex items-center gap-4">
        <UiDataMedia
          :label="sideInfo.fighter.settlement.name"
          :icon="`crpg:${settlementIconByType[sideInfo.fighter.settlement.type]}`"
          size="md"
        />

        <UserMedia
          v-if="sideInfo.fighter.settlement.owner"
          :user="sideInfo.fighter.settlement.owner"
        />
      </div>
    </template>

    <!-- TODO: -->
    <UButton label="Manage" icon="crpg:settings" @click="$emit('openManage')" />
  </div>
</template>
