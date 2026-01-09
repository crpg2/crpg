<script setup lang="ts">
import type { BattleSide, BattleSideDetailed } from '~/models/strategus/battle'

import { BATTLE_SIDE } from '~/models/strategus/battle'

defineProps<{
  side: BattleSide
  sideInfo: BattleSideDetailed
}>()
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
      <slot name="topbar-prepend" />

      <UiTextView variant="caption-sm">
        {{ $t(`strategus.battle.side.${side}`) }}
      </UiTextView>

      <UTooltip text="Tickets count">
        <UBadge icon="crpg:member" :label="$n(sideInfo.totalTroops)" variant="subtle" />
      </UTooltip>
    </div>

    <template v-if="sideInfo.settlement">
      <template v-if="sideInfo.settlement.owner?.clanMembership?.clan">
        <UiDataMedia
          :label="sideInfo.settlement.owner.clanMembership.clan.name"
          size="xl"
        >
          <template #icon="{ classes: clanTagIconClasses }">
            <ClanTagIcon
              :color="sideInfo.settlement.owner.clanMembership.clan.primaryColor"
              :class="clanTagIconClasses()"
            />
          </template>
        </UiDataMedia>
      </template>

      <div class="flex items-center gap-4">
        <SettlementMedia :settlement="sideInfo.settlement" />

        <UserMedia
          v-if="sideInfo.settlement.owner"
          :user="sideInfo.settlement.owner"
        />
      </div>
    </template>

    <template v-else-if="sideInfo.commander.party">
      <UiDataMedia
        v-if="sideInfo.commander.party.user.clanMembership?.clan"
        :label="sideInfo.commander.party.user.clanMembership.clan.name"
        size="xl"
        :layout="side === BATTLE_SIDE.Attacker ? 'reverse' : 'normal'"
        :style="{ color: sideInfo.commander.party.user.clanMembership.clan.primaryColor }"
      >
        <template #icon="{ classes: clanTagIconClasses }">
          <ClanTagIcon
            :color="sideInfo.commander.party.user.clanMembership.clan.primaryColor"
            :class="clanTagIconClasses()"
          />
        </template>
      </UiDataMedia>

      <UserMedia :user="sideInfo.commander.party.user" />
    </template>

    <slot name="append" />
  </div>
</template>
