<script setup lang="ts">
import type { EventContentArg } from '@fullcalendar/core'

const props = defineProps<{ arg: EventContentArg }>()

const battle = props.arg.event.extendedProps.battle

// const attacker = battle.attacker
// const defender = battle.defender
</script>

<template>
  <div
    class="rounded border border-gray-700 bg-gray-900 p-1 text-xs leading-tight text-white shadow"
  >
    <div class="font-bold">
      ⚔️ Битва
    </div>

    <div class="mt-1 flex justify-between gap-1">
      <div class="flex-1 text-left">
        <div class="font-semibold text-green-300">
          Атакует:
        </div>
        <!-- <div v-if="attacker.party">
          {{ attacker.party.name }} ({{ attacker.party.troops }})
        </div>
        <div v-else-if="attacker.settlement">
          🏰 {{ attacker.settlement.name }} ({{ attacker.settlement.troops }})
        </div> -->
      </div>

      <div class="px-1 text-center font-bold text-red-400">
        VS
      </div>

      <div class="flex-1 text-right">
        <div class="font-semibold text-red-300">
          Оборона:
        </div>
        <!-- <div v-if="defender.party">
          {{ defender.party.name }} ({{ defender.party.troops }})
        </div>
        <div v-else-if="defender.settlement">
          🏰 {{ defender.settlement.name }} ({{ defender.settlement.troops }})
        </div> -->
      </div>
    </div>
  </div>
</template>
