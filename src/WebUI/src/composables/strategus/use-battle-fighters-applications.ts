import { useAsyncState } from '@vueuse/core'

import { BattleFighterApplication, BattleFighterApplicationStatus } from '~/models/strategus/battle'
import { getBattleFighterApplications } from '~/services/strategus-service/battle-service'

export const useBattleFighterApplications = () => {
  const { execute: loadBattleFighterApplications, state: fighterApplications } = useAsyncState(
    ({ id }: { id: number }) =>
      getBattleFighterApplications(id, [BattleFighterApplicationStatus.Pending]),
    [],
    {
      immediate: false,
    },
  )

  const fighterApplicationsCount = computed(() => fighterApplications.value.length)

  return {
    fighterApplicationsCount,
    fighterApplications,
    loadBattleFighterApplications,
  }
}
