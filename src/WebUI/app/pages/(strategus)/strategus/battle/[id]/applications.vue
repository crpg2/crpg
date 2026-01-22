<script setup lang="ts">
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

    // onResponded()

    toast.add({
      title: status
        ? t('strategus.battle.manage.mercenaryApplications.respond.accept.notify.success') // TODO: FIXME: mercenaryApplications => battleFighterApplications
        : t('strategus.battle.manage.mercenaryApplications.respond.decline.notify.success'), // TODO: FIXME:
      close: false,
      color: 'success',
    })
  },
)
</script>

<template>
  <div>
    <BattleManageFighterApplicationsTable
      :applications="fighterApplications"
      :loading="loadingBattleFighterApplications"
      @respond="onRespond"
    />
  </div>
</template>
