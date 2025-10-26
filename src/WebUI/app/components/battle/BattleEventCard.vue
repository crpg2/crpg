<script setup lang="ts">
import type { Battle } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType, getBattleTitle } from '~/services/strategus/battle-service'

const { battle } = defineProps<{ battle: Battle }>()
const { clan } = useUser()

const battleTitle = computed(() => getBattleTitle(battle))

// TODO: подсвечивать бои
const rowClass = (battle: Battle) => {
  const userClanId = clan.value?.id

  //  TODO: FIXME:
  const isClanBattle = [
    battle.attacker.party?.user?.clanMembership?.clan.id,
    battle.defender?.party?.user?.clanMembership?.clan.id,
    battle.defender?.settlement?.owner?.clanMembership?.clan.id,
  ]
    .filter(Boolean)
    .includes(userClanId)

  return isClanBattle ? 'text-primary' : 'text-content-100'
}
</script>

   <!-- :style="[
      {
        ...({
          backgroundColor: `color-mix(in srgb, #000 10%, var(--color-success) 35%)`,
        }),
      },
    ]" -->
<template>
  <UCard
    variant="subtle"
    :ui="{
      header: 'flex justify-between items-center gap-4',
      footer: 'flex justify-between items-center gap-4',
    }"
  >
    <template #header>
      <UiTextView variant="h4" tag="h5">
        {{ battleTitle }}
      </UiTextView>

      <BattlePhaseBadge :phase="battle.phase" />
    </template>

    <div class="flex justify-center gap-6">
      <BattleSideView
        :side="BATTLE_SIDE.Attacker"
        :fighter="battle.attacker"
        :total-troops="battle.attackerTotalTroops"
      />

      <UTooltip :text="battle.type" :content="{ side: 'top' }">
        <USeparator
          orientation="vertical"
          class="h-28 self-center"
          size="sm"
          :icon="`crpg:${battleIconByType[battle.type]}`"
          :ui="{
            icon: 'size-7',
          }"
        />
      </UTooltip>

      <BattleSideView
        :side="BATTLE_SIDE.Defender"
        :fighter="battle.defender!"
        :total-troops="battle.defenderTotalTroops"
      />
    </div>

    <template #footer>
      <UiTextView variant="caption">
        {{ $t(`region.${battle.region}`) }} · {{ $t(`strategus.battle.type.${battle.type}`) }} · <template v-if="battle.scheduledFor">
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
