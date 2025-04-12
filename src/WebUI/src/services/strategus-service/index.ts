import type { StrategusUpdate } from '~/models/strategus'
import type { Party, PartyStatusUpdateRequest } from '~/models/strategus/party'

import { PartyStatus } from '~/models/strategus/party'
import { post, put, tryGet } from '~/services/crpg-client'

export const getUpdate = () => tryGet<StrategusUpdate>('/parties/self/update')

export const updatePartyStatus = (update: PartyStatusUpdateRequest) =>
  put<Party>('/parties/self/status', update)

export const registerUser = () => post<Party>('/parties', {}) // TODO: empty obj?

export const inSettlementStatuses = new Set<PartyStatus>([
  PartyStatus.IdleInSettlement,
  PartyStatus.RecruitingInSettlement,
])
