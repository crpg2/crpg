<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import { groupBy } from 'es-toolkit'

import { useBattleFighterApplications, useMapBattle } from '~/composables/strategus/map/use-map-battle'
import { usePartyState } from '~/composables/strategus/use-party'
import { BATTLE_SIDE } from '~/models/strategus/battle'

const { party } = usePartyState().value
const toast = useToast()
const { t, n } = useI18n()
const { copy } = useClipboard()
// const battle = computed(() => party.targetedBattle!)

// const battleCoordinates = computed(() => battle.value.position.coordinates.map(p => n(p)).join(', '))

// const onPositionCopy = () => {
//   copy(battleCoordinates.value)
//   toast.add({
//     title: t('action.copied'),
//     close: false,
//     color: 'success',
//   })
// }

// const fightersBySide = computed(() => groupBy(battle.value.fighters, f => f.side))
const { battle, refreshBattle, battleTitle } = useMapBattle()

const { fighterApplications } = useBattleFighterApplications()
</script>

<template>
  <div>
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

    <div v-for="app in fighterApplications" :key="app.id">
      <UserMedia :user="app.party.user" />
    </div>
  </div>
</template>
