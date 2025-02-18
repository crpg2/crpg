<script setup lang="ts">
import type { Battle, BattleFighter } from '~/models/strategus/battle'

import { BattleSide } from '~/models/strategus/battle'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  battle: Battle
  fighters: BattleFighter[]
  side: BattleSide
}>()

const userStore = useUserStore()
const isSelfUser = (row: BattleFighter) => row.party?.user.id === userStore.user?.id

const rowClass = (row: BattleFighter): string =>
  isSelfUser(row) ? 'text-primary' : 'text-content-100'

const sideCommander = computed(() => {
  if (props.side === BattleSide.Attacker) {
    return props.battle.attacker.party?.user
  }
  else if (props.battle.defender?.party) {
    return props.battle.defender?.party?.user
  }
  else {
    return props.battle.defender?.settlement?.owner?.user
  }
})

const sideFighters = computed(() =>
  props.fighters.filter(fighter => fighter.side === props.side && !fighter.commander))
</script>

<template>
  <div class=" text-content-100">
    <h1 class="mb-8 text-center text-xl">
      {{ $t('strategus.battle.side.'.concat(side).toLowerCase()) }}
    </h1>
    <div v-if="sideCommander" class="flex items-center gap-1 pb-4">
      <UserMedia
        :user="sideCommander"
        hidden-platform
        size="xl"
      />
      <OIcon
        v-tooltip.right="$t('strategus.battle.tooltip.commander')"
        icon="trophy-cup"
        size="lg"
      />
    </div>
    <div
      v-for="fighter in sideFighters"
      :key="fighter.id"
      class="flex flex-col gap-3 pb-4 "
    >
      <div v-if="fighter.party?.user">
        <UserMedia
          :user="fighter.party.user"
          hidden-platform
          size="xl"
          :class="rowClass(fighter)"
        />
      </div>
      <div v-if="fighter.settlement?.owner">
        <UserMedia
          :user="fighter.settlement.owner.user"
          hidden-platform
          size="xl"
        />
      </div>
    </div>
  </div>
</template>
