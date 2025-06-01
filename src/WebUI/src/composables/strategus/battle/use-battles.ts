import { BattlePhase } from '~/models/strategus/battle'
import { getBattle } from '~/services/strategus-service/battle'

export const useBattles = () => {
  const route = useRoute()
  const router = useRouter()

  // TODO: vue-use query
  const battlePhaseModel = computed({
    get() {
      return (route.query?.battlePhase as BattlePhase[]) || [BattlePhase.Scheduled, BattlePhase.Hiring]
    },

    set(battlePhases: BattlePhase[]) {
      router.replace({
        query: {
          ...route.query,
          battlePhases,
        },
      })
    },
  })

  const battlePhases = Object.values(BattlePhase)

  return {
    battlePhaseModel,
    battlePhases,
  }
}

export const useBattle = () => {
  const { state: battle, execute: loadBattle } = useAsyncState(
    ({ id }: { id: number }) => getBattle(id),
    null,
    {
      immediate: false,
    },
  )

  return {
    battle,
    loadBattle,
  }
}
