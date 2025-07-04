import { mockGet, mockPost, mockPut } from 'vi-fetch'

import { response } from '~/__mocks__/crpg-client'
import { PartyStatus } from '~/models/strategus/party'

import { getUpdate, registerUser, updatePartyStatus } from './index'

it('registerUser', async () => {
  mockPost('/parties').willResolve(response('ok'))
  expect(await registerUser()).toEqual('ok')
})

it('getUpdate', async () => {
  mockGet('/parties/self/update').willResolve(response('ok'))
  expect(await getUpdate()).toEqual(response('ok'))
})

it('updatePartyStatus', async () => {
  mockPut('/parties/self/status').willResolve(response('ok'))
  expect(
    await updatePartyStatus({
      status: PartyStatus.MovingToPoint,
      targetedPartyId: 0,
      targetedSettlementId: 0,
      waypoints: { coordinates: [], type: 'MultiPoint' },
    }),
  ).toEqual('ok')
})
