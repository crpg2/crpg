import type { UpdatePartyStatusCommandWritable } from '#api'

import {
  getPartiesSelfUpdate,
  postParties,
  putPartiesSelfStatus,
} from '#api/sdk.gen'

import type { PartyStatus } from '~/models/strategus/party'

import { PARTY_STATUS } from '~/models/strategus/party'

export const getSelfUpdate = () => getPartiesSelfUpdate({})

export const updatePartyStatus = async (payload: UpdatePartyStatusCommandWritable) => (await putPartiesSelfStatus({ body: payload })).data!

export const registerParty = () => postParties({})

export const inSettlementStatuses = new Set<PartyStatus>([
  PARTY_STATUS.IdleInSettlement,
  PARTY_STATUS.RecruitingInSettlement,
])
