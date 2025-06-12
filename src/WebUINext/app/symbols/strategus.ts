import type { DeepReadonly } from 'vue'

import type { Party, PartyStatusUpdateRequest } from '~/models/strategus/party'
import type { SettlementPublic } from '~/models/strategus/settlement'

export const settlementKey: InjectionKey<DeepReadonly<Ref<SettlementPublic | null>>> = Symbol('Settlement')

export const partyKey: InjectionKey<{
  party: DeepReadonly<Ref<Party | null>>
  toggleRecruitTroops: () => void
  isTogglingRecruitTroops: DeepReadonly<Ref<boolean>>
  moveParty: (updateRequest: Partial<PartyStatusUpdateRequest>) => Promise<void>
}> = Symbol('Party')
