<script setup lang="ts">
import { LIcon, LMarker, LTooltip } from '@vue-leaflet/vue-leaflet'

import type { BattleFighter, BattleSide, MapBattle } from '~/models/strategus/battle'

import { useMapContext } from '~/composables/strategus/use-map'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_SIDE } from '~/models/strategus/battle'
import { positionToLatLng } from '~/utils/geometry'

const { battle } = defineProps<{ battle: MapBattle }>()

defineEmits<{ join: [side: BattleSide] }>()

const { zoom } = useMapContext()

const showDetail = computed(() => zoom.value > 5)

const { user } = useUser()
const attackerCommander = computed(() => battle.fighters.find(f => f.commander && f.side === BATTLE_SIDE.Attacker))
const defenderCommander = computed(() => battle.fighters.find(f => f.commander && f.side === BATTLE_SIDE.Defender))

const fighterTroopsAcc = (fighter: BattleFighter): number => {
  if (fighter.party) {
    return fighter.party.troops
  }

  if (fighter.settlement) {
    return fighter.settlement.troops
  }

  return 0
}

// TODO: вынести на бек
const totalTroopsBySide = (side: BattleSide) => computed(() => battle.fighters
  .filter(f => f.side === side)
  .reduce((acc, f) => acc + fighterTroopsAcc(f), 0))

const attackerTotalTroops = totalTroopsBySide(BATTLE_SIDE.Attacker)
const defenderTotalTroops = totalTroopsBySide(BATTLE_SIDE.Defender)
</script>

<template>
  <LMarker
    :lat-lng="positionToLatLng(battle.position.coordinates)"
    :options="{ bubblingMouseEvents: false }"
  >
    <LIcon class-name="!flex justify-center items-center">
      <div
        class="
          flex items-center justify-center gap-2 rounded-md bg-error/66 p-2 text-highlighted
          hover:ring hover:ring-inverted
        "
      >
        <UserMedia
          :user="attackerCommander!.party!.user"
          hidden-clan
          hidden-platform
          :hidden-title="!showDetail"
          :is-self="user!.id === attackerCommander?.party?.user.id"
        />

        <UIcon name="crpg:game-mode-duel" :class="zoom > 5 ? 'size-7' : 'size-5'" />

        <UserMedia
          :user="defenderCommander!.party!.user"
          hidden-clan
          hidden-platform
          :hidden-title="!showDetail"
          :is-self="user!.id === defenderCommander?.party?.user.id"
        />
      </div>
    </LIcon>

    <LTooltip :options="{ direction: 'top', offset: [0, -32] }">
      <div class="flex gap-4 p-2">
        <UiDataCell>
          <template #leftContent>
            <UserMedia
              :user="attackerCommander!.party!.user"
              :is-self="user!.id === attackerCommander?.party?.user.id"
            />
          </template>
          <UiDataMedia icon="crpg:member" :label="$n(attackerTotalTroops)" />
        </UiDataCell>

        <UIcon name="crpg:game-mode-duel" :class="zoom > 5 ? 'size-7' : 'size-5'" />

        <UiDataCell>
          <template #leftContent>
            <UserMedia
              :user="defenderCommander!.party!.user"
              :is-self="user!.id === defenderCommander?.party?.user.id"
            />
          </template>
          <UiDataMedia icon="crpg:member" :label="$n(defenderTotalTroops)" />
        </UiDataCell>
      </div>
    </LTooltip>
  </LMarker>
</template>
