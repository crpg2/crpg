<script setup lang="ts">
import type { EventContentArg } from '@fullcalendar/core'

import type { Battle } from '~/models/strategus/battle'

const { battle } = defineProps<{ battle: Battle }>()

// const attacker = battle.attacker
// const defender = battle.defender
</script>

<template>
  <UCard
    variant="subtle"
    :ui="{
      footer: 'flex items-end gap-4',
      body: 'relative ',
      root: 'h-full',
    }"
  >
    <template #header>
      <div class="flex items-center gap-2">
        <div>
          {{ $d(battle.scheduledFor!, 'short') }}
        </div>
        <div>
          {{ $t(`strategus.battle.phase.${battle.phase.toLowerCase()}`) }}
        </div>
      </div>
    </template>

    <div class="mt-1 flex justify-between gap-1">
      <div class="flex-1 text-left">
        <div class="font-semibold text-green-300">
          Атакует:
        </div>
        <div v-if="battle.attacker.party">
          <UserMedia :user="battle.attacker.party.user" hidden-platform />
          {{ battle.attackerTotalTroops }}
        </div>
      </div>

      <div class="px-1 text-center font-bold text-red-400">
        VS
      </div>

      <div class="flex-1 text-right">
        <div class="font-semibold text-red-300">
          Оборона:
        </div>
        <div v-if="battle?.defender?.party">
          {{ battle.defender.party.name }} ({{ battle.defenderTotalTroops }})
        </div>
        <div v-else-if="battle?.defender?.settlement">
          🏰 {{ battle.defender.settlement.name }} {{ battle.defenderTotalTroops }}
        </div>
      </div>
    </div>

    <template #footer>
      ddd
    </template>
  </UCard>
  <!-- <div
    class="rounded border border-gray-700 bg-gray-900 p-1 text-xs leading-tight text-white shadow"
  >
    <div class="font-bold">
      {{ $d(battle.scheduledFor!, 'short') }} {{ $t(`strategus.battle.phase.${battle.phase.toLowerCase()}`) }}
    </div>

  </div> -->
</template>
