<script setup lang="ts">
import { LazyBattleManageFigterItemsDrawer } from '#components'

import { useBattleFighterApplications } from '~/composables/strategus/map/use-map-battle'

const {
  fighterApplications,
  loadingBattleFighterApplications,
  loadBattleFighterApplications,
  respondToBattleFighterApplication,
} = useBattleFighterApplications()

const toast = useToast()
const { t } = useI18n()

const [onRespond, responding] = useAsyncCallback(
  async (applicationId: number, status: boolean) => {
    await respondToBattleFighterApplication(applicationId, status)
    await loadBattleFighterApplications()

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
  <div>
    <BattleManageFighterApplicationsTable
      :applications="fighterApplications"
      :loading="loadingBattleFighterApplications || responding"
      @respond="onRespond"
      @show-items="(partyId) => partyFighterItemDrawer.open({ party: getFighterApplicationByPartyId(partyId)?.party })"
    />
  </div>
</template>
