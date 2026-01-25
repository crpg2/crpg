<script setup lang="ts">
import { LazyBattleManageFigterItemsDrawer } from '#components'

import { useBattleFighterApplications, useMapBattle } from '~/composables/strategus/map/use-map-battle'
import { usePartyState } from '~/composables/strategus/use-party'

definePageMeta({
  middleware: [
    () => {
      const { partyState } = usePartyState()
      const { battle } = useMapBattle()
      const commander = partyState.value.party.targetedBattle?.fighters.some(f => f.commander && f.party?.id === partyState.value.party.id)

      if (!commander) {
        return navigateTo({ name: 'strategus-battle-id', params: { id: battle.value.id } })
      }
    },
  ],
})

const {
  fighterApplications,
  refreshFighterApplications,
  respondToBattleFighterApplication,
} = useBattleFighterApplications()

const toast = useToast()
const { t } = useI18n()

const [onRespond, responding] = useAsyncCallback(
  async (applicationId: number, status: boolean) => {
    await respondToBattleFighterApplication(applicationId, status)
    await refreshFighterApplications()

    toast.add({
      title: status
        ? t('strategus.battle.manage.mercenaryApplications.respond.accept.notify.success') // TODO: FIXME: mercenaryApplications => battleFighterApplications
        : t('strategus.battle.manage.mercenaryApplications.respond.decline.notify.success'), // TODO: FIXME:
      close: false,
      color: 'success',
    })
  },
)

const overlay = useOverlay()

const partyFighterItemDrawer = overlay.create(LazyBattleManageFigterItemsDrawer)

function getFighterApplicationByPartyId(partyId: number) {
  return fighterApplications.value.find(a => a.party.id === partyId)
}
</script>

<template>
  <BattleManageFighterApplicationsTable
    :applications="fighterApplications"
    :loading="responding"
    @respond="onRespond"
    @show-items="(partyId) => partyFighterItemDrawer.open({ party: getFighterApplicationByPartyId(partyId)?.party! })"
  />
</template>
