import {
  getPartiesSelfUpdate,
  postParties,
  putPartiesSelfStatus,
} from '#api/sdk.gen'

import type { CrpgApiResult } from '~/api.config'
import type { PartyStatus, StrategusUpdate, UpdatePartyStatus } from '~/models/strategus/party'

import { PARTY_STATUS } from '~/models/strategus/party'

export const getSelfUpdate = (): Promise<CrpgApiResult<StrategusUpdate>> => getPartiesSelfUpdate({})

export const updatePartyStatus = async (payload: UpdatePartyStatus) => (await putPartiesSelfStatus({ body: payload })).data!

export const registerParty = () => postParties({ body: {} })

export const IN_SETTLEMENT_PARTY_STATUSES: PartyStatus[] = [
  PARTY_STATUS.IdleInSettlement,
  PARTY_STATUS.RecruitingInSettlement,
]
