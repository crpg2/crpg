import { BattleSide } from '~/models/strategus/battle'
import { getBattleFighters } from '~/services/strategus-service/battle-service'

export const useBattleFighters = () => {
  const { execute: loadBattleFighters, isLoading: battleFightersLoading, state: battleFighters } = useAsyncState(
    ({ id }: { id: number }) => getBattleFighters(id),
    [],
    {
      immediate: false,
    },
  )

  const battleFightersCount = computed(() => battleFighters.value.length)

  return {
    battleFightersLoading,
    battleFighters,
    battleFightersCount,
    loadBattleFighters,
  }
}
