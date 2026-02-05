<script setup lang="ts">
import type { BattleFighter, BattleSide } from '~/models/strategus/battle'
import type { SettlementPublic } from '~/models/strategus/settlement'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_SIDE } from '~/models/strategus/battle'

defineProps<{
  side: BattleSide
  commander: BattleFighter
  settlement: SettlementPublic | null
  totalTroops: number
}>()

const { user } = useUser()
</script>

<template>
  <div
    class="flex flex-1 flex-col gap-y-3.5"
    :class="side === BATTLE_SIDE.Attacker
      ? 'items-end text-right'
      : 'items-start text-left'"
  >
    <div
      class="flex items-center gap-2"
      :class="side === BATTLE_SIDE.Attacker
        ? 'flex-row justify-end'
        : 'flex-row-reverse justify-end'"
    >
      <slot name="topbar-prepend" />

      <UiTextView variant="caption-sm">
        {{ $t(`strategus.battle.side.${side}`) }}
      </UiTextView>

      <!-- TODO: FIXME: i18n -->
      <UTooltip text="Tickets count">
        <UBadge icon="crpg:member" :label="$n(totalTroops)" variant="subtle" />
      </UTooltip>
    </div>

    <template v-if="settlement">
      <template v-if="settlement.owner?.clanMembership?.clan">
        <UiDataMedia
          :label="settlement.owner.clanMembership.clan.name"
          size="xl"
        >
          <template #icon="{ classes: clanTagIconClasses }">
            <ClanTagIcon
              :color="settlement.owner.clanMembership.clan.primaryColor"
              :class="clanTagIconClasses()"
            />
          </template>
        </UiDataMedia>
      </template>

      <div class="flex items-center gap-4">
        <SettlementMedia :settlement />

        <UserMedia
          v-if="settlement.owner"
          :user="settlement.owner"
          :is-self="user!.id === settlement.owner.id"
        />
      </div>
    </template>

    <template v-else-if="commander.party">
      <UiDataMedia
        v-if="commander.party.user.clanMembership?.clan"
        :label="commander.party.user.clanMembership.clan.name"
        size="xl"
        :layout="side === BATTLE_SIDE.Attacker ? 'reverse' : 'normal'"
        :style="{ color: commander.party.user.clanMembership.clan.primaryColor }"
      >
        <template #icon="{ classes: clanTagIconClasses }">
          <ClanTagIcon
            :color="commander.party.user.clanMembership.clan.primaryColor"
            :class="clanTagIconClasses()"
          />
        </template>
      </UiDataMedia>

      <UserMedia :user="commander.party.user" :is-self="user!.id === commander.party.user.id" />
    </template>

    <slot name="append" />
  </div>
</template>
