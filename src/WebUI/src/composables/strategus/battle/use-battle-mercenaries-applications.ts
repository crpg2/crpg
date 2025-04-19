import { useAsyncState } from '@vueuse/core'

import { BattleMercenaryApplication, BattleMercenaryApplicationStatus } from '~/models/strategus/battle'
import { getBattleMercenaryApplications } from '~/services/strategus-service/battle'

export const useBattleMercenaryApplications = () => {
  const { state: mercenaryApplications, execute: loadBattleMercenaryApplications } = useAsyncState(
    ({ id }: { id: number }) => getBattleMercenaryApplications(id, [BattleMercenaryApplicationStatus.Pending]),
    [],
    {
      immediate: false,
    },
  )

  const mercenaryApplicationsCount = computed(() => mercenaryApplications.value.length)

  return {
    mercenaryApplicationsCount,
    mercenaryApplications,
    loadBattleMercenaryApplications,
  }
}
