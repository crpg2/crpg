<script setup lang="ts">
import type { Battle, BattleMercenaryApplicationStatus } from '~/models/strategus/battle'

import { useBattleTitle } from '~/composables/strategus/battle/use-battle'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_MERCENARY_APPLICATION_STATUS } from '~/models/strategus/battle'

const { battle } = defineProps<{ battle: Battle }>()
const { clan, user } = useUser()

const battleTitle = useBattleTitle(battle)

// TODO: подсвечивать бои
// const rowClass = (battle: Battle) => {
//   const userClanId = clan.value?.id

//   //  TODO: FIXME:
//   const isClanBattle = [
//     battle.attacker.commander.party?.user?.clanMembership?.clan.id,
//     battle.defender.commander.party?.user?.clanMembership?.clan.id,
//     battle.defender.commander.settlement?.owner?.clanMembership?.clan.id,
//   ]
//     .filter(Boolean)
//     .includes(userClanId)

//   return isClanBattle
//     ? 'text-primary'
//     : 'text-content-100'
// }

function getCardStyleByApplicationStatus(status: BattleMercenaryApplicationStatus) {
  const cardColorByApplicationStatus: Record<BattleMercenaryApplicationStatus, string> = {
    Pending: `oklch(70.7% 0.165 254.624)`, // TODO:
    Accepted: '#53bc96',
    Declined: '#CA4949',
  }

  return {
    '--tw-ring-color': cardColorByApplicationStatus[status],
    'backgroundColor': `color-mix(in srgb, #000 100%, ${cardColorByApplicationStatus[status]} 15%)`,
  }
}

const cardStyle = computed(() => {
  // TODO: need a name
  const isSelfBattle = [
    battle.attacker.commander.party?.user.id,
    battle.defender.commander.party?.user.id,
  ]
    .filter(Boolean)
    .includes(user.value!.id)

  // TODO:
  if (isSelfBattle) {
    return {
      '--tw-ring-color': 'var(--color-gold)',
      'backgroundColor': `color-mix(in srgb, #000 100%, var(--color-gold) 15%)`,
    }
  }

  const statuses = [
    battle.attacker.mercenaryApplication?.status,
    battle.defender.mercenaryApplication?.status,
  ].filter(Boolean) as BattleMercenaryApplicationStatus[]

  if (!statuses.length) {
    return undefined
  }

  const accepted = statuses.find(s => s === BATTLE_MERCENARY_APPLICATION_STATUS.Accepted)
  if (accepted) {
    return getCardStyleByApplicationStatus(accepted)
  }

  if (statuses.length === 2 && statuses.includes(BATTLE_MERCENARY_APPLICATION_STATUS.Declined)) {
    const other = statuses.find(s => s !== BATTLE_MERCENARY_APPLICATION_STATUS.Declined)
    if (other) {
      return getCardStyleByApplicationStatus(other)
    }
  }

  return getCardStyleByApplicationStatus(statuses.at(0)!)
})
</script>

<template>
  <UCard
    variant="subtle"
    :ui="{
      header: 'flex justify-between items-center gap-4',
      footer: 'flex justify-between items-center gap-4',
    }"
    class=""
    :style="cardStyle"
  >
    <template #header>
      <UiTextView variant="h4" tag="h5">
        {{ battleTitle }}
      </UiTextView>

      <BattlePhaseBadge :phase="battle.phase" />
    </template>

    <BattleSideViewGroup
      :battle
      :can-apply="{
        Attacker: null,
        Defender: null,
      }"
      :can-manage="{
        Attacker: false,
        Defender: false,
      }"
    />

    <template #footer>
      <UiTextView variant="caption">
        {{ $t(`region.${battle.region}`, 0) }} · {{ $t(`strategus.battle.type.${battle.type}`) }} · <template v-if="battle.scheduledFor">
          {{ $d(battle.scheduledFor, 'short') }}
        </template>
      </UiTextView>

      <!-- <UBadge
        v-if="battle.scheduledFor"
        :label="$d(battle.scheduledFor, 'short')"
        variant="subtle"
        color="neutral"
      /> -->

      <UButton
        icon="i-lucide-arrow-right" trailing
        variant="subtle" color="neutral" label="Detail"
        size="xl"
        :to="{ name: 'battles-id', params: { id: battle.id } }"
      />
    </template>
  </UCard>
</template>
