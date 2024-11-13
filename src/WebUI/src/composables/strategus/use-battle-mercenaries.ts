import { BattleSide } from '~/models/strategus/battle'
import { getBattleMercenaries } from '~/services/strategus-service/battle-service'

export const useBattleMercenaries = () => {
  const { execute: loadBattleMercenaries, isLoading: battleMercenariesLoading, state: battleMercenaries } = useAsyncState(
    ({ id }: { id: number }) => getBattleMercenaries(id),
    [],
    {
      immediate: false,
    },
  )

  const battleMercenariesCount = computed(() => battleMercenaries.value.length)

  const battleMercenariesAttackers = computed(() =>
    battleMercenaries.value.filter(mercenary => mercenary.side === BattleSide.Attacker))

  const battleMercenariesDefenders = computed(() =>
    battleMercenaries.value.filter(mercenary => mercenary.side === BattleSide.Defender))

  return {
    battleMercenariesLoading,
    battleMercenaries,
    battleMercenariesCount,
    battleMercenariesAttackers,
    battleMercenariesDefenders,
    loadBattleMercenaries,
  }
}
