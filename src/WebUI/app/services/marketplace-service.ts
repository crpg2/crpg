import { client } from '#api/client.gen'

import type { CrpgApiResult } from '~/api.config'
import type { CreateMarketplaceOfferPayload, MarketplaceOffer } from '~/models/marketplace'

const MARKETPLACE_SECURITY = [{ scheme: 'bearer', type: 'http' }] as const

function unwrapData<T>(result: CrpgApiResult<T>): T {
  if (result.data == null) {
    throw new Error('Marketplace response contains no data')
  }

  return result.data
}

export const getMarketplaceOffers = async (requestedItemId?: string): Promise<MarketplaceOffer[]> => {
  const result = await client.get({
    security: MARKETPLACE_SECURITY,
    url: '/Marketplace/offers',
    ...(requestedItemId ? { query: { requestedItemId } } : {}),
  }) as CrpgApiResult<MarketplaceOffer[]>

  return unwrapData(result)
}

export const getSelfMarketplaceOffers = async (): Promise<MarketplaceOffer[]> => {
  const result = await client.get({
    security: MARKETPLACE_SECURITY,
    url: '/Marketplace/offers/self',
  }) as CrpgApiResult<MarketplaceOffer[]>

  return unwrapData(result)
}

export const createMarketplaceOffer = async (payload: CreateMarketplaceOfferPayload): Promise<MarketplaceOffer> => {
  const result = await client.post({
    security: MARKETPLACE_SECURITY,
    url: '/Marketplace/offers',
    body: payload,
  }) as CrpgApiResult<MarketplaceOffer>

  return unwrapData(result)
}

export const cancelMarketplaceOffer = (offerId: number): Promise<void> =>
  client.delete({
    security: MARKETPLACE_SECURITY,
    url: `/Marketplace/offers/${offerId}`,
  }) as Promise<void>

export const acceptMarketplaceOffer = (offerId: number): Promise<void> =>
  client.post({
    security: MARKETPLACE_SECURITY,
    url: `/Marketplace/offers/${offerId}/accept`,
  }) as Promise<void>
