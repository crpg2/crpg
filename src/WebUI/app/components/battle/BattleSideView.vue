<script setup lang="ts">
import type { BattleFighter, BattleSide } from '~/models/strategus/battle'

import { BATTLE_SIDE } from '~/models/strategus/battle'
import { settlementIconByType } from '~/services/strategus/settlement-service'

defineProps<{
  side: BattleSide
  fighter: BattleFighter
  totalTroops: number
}>()
</script>

<template>
  <div
    class="flex flex-1 flex-col gap-y-3.5"
    :class="side === BATTLE_SIDE.Attacker ? 'items-end text-right' : 'text-left'"
  >
    <div
      class="flex items-center gap-2"
      :class="side === BATTLE_SIDE.Attacker ? 'flex-row justify-end' : `
        flex-row-reverse justify-end
      `"
    >
      <UiTextView variant="caption-sm">
        {{ $t(`strategus.battle.side.${side}`) }}
      </UiTextView>
      <UBadge icon="crpg:member" :label="$n(totalTroops)" variant="subtle" />
    </div>

    <template v-if="fighter.party">
      <UiDataMedia
        v-if="fighter.party.user.clanMembership?.clan"
        :label="fighter.party.user.clanMembership.clan.name"
        size="xl"
        :class="side === BATTLE_SIDE.Attacker ? 'flex-row-reverse' : ''"
        :style="{ color: fighter.party.user.clanMembership.clan.primaryColor }"
      >
        <template #icon="{ classes: clanTagIconClasses }">
          <ClanTagIcon
            :color="fighter.party.user.clanMembership.clan.primaryColor"
            :class="clanTagIconClasses()"
          />
        </template>
      </UiDataMedia>

      <UserMedia :user="fighter.party.user" />
    </template>

    <template v-else-if="fighter.settlement">
      <template v-if="fighter.settlement.owner?.clanMembership?.clan">
        <UiDataMedia
          :label="fighter.settlement.owner.clanMembership.clan.name"
          size="xl"
        >
          <template #icon="{ classes: clanTagIconClasses }">
            <ClanTagIcon
              :color="fighter.settlement.owner.clanMembership.clan.primaryColor"
              :class="clanTagIconClasses()"
            />
          </template>
        </UiDataMedia>
      </template>

      <div class="flex items-center gap-4">
        <UiDataMedia
          :label="fighter.settlement.name"
          :icon="`crpg:${settlementIconByType[fighter.settlement.type]}`"
          size="md"
        />

        <UserMedia
          v-if="fighter.settlement.owner"
          :user="fighter.settlement.owner"
        />
      </div>
    </template>
  </div>
</template>
