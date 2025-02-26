import { useAsyncState } from '@vueuse/core'

import { BattleMercenaryApplication, BattleMercenaryApplicationStatus } from '~/models/strategus/battle'
import { getBattleMercenaryApplications } from '~/services/strategus-service/battle-service'

export const useBattleMercenaryApplications = () => {
  const { execute: loadBattleMercenaryApplications, state: mercenaryApplications } = useAsyncState(
    ({ id }: { id: number }) =>
      getBattleMercenaryApplications(id, [BattleMercenaryApplicationStatus.Pending]),
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
