import type { PartialDeep } from 'type-fest'

import { describe, expect, it, vi } from 'vitest'

import type { MarketplaceOffer, MarketplaceOfferAssetInput } from '~/models/marketplace'
import type { User, UserItem } from '~/models/user'

import { ITEM_TYPE } from '~/models/item'
import { MARKETPLACE_ASSET_SIDE } from '~/models/marketplace'

import * as marketplaceService from '../marketplace-service'

const {
  mockedDeleteMarketplaceOffersByOfferId,
  mockedGetMarketplaceOffers,
  mockedGetMarketplaceOffersHistory,
  mockedPostMarketplaceOffers,
  mockedPostMarketplaceOffersByOfferIdAccept,
} = vi.hoisted(() => ({
  mockedDeleteMarketplaceOffersByOfferId: vi.fn(),
  mockedGetMarketplaceOffers: vi.fn(),
  mockedGetMarketplaceOffersHistory: vi.fn(),
  mockedPostMarketplaceOffers: vi.fn(),
  mockedPostMarketplaceOffersByOfferIdAccept: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  deleteMarketplaceOffersByOfferId: mockedDeleteMarketplaceOffersByOfferId,
  getMarketplaceOffers: mockedGetMarketplaceOffers,
  getMarketplaceOffersHistory: mockedGetMarketplaceOffersHistory,
  postMarketplaceOffers: mockedPostMarketplaceOffers,
  postMarketplaceOffersByOfferIdAccept: mockedPostMarketplaceOffersByOfferIdAccept,
}))

vi.mock('~/api.config', () => ({
  unwrapData: (result: { data: unknown }) => result.data,
}))

describe('marketplace service', () => {
  describe('getMarketplaceOffers', () => {
    it('converts 0-based pageIndex to 1-based page and passes all filter params', async () => {
      const offersPage = { items: [], totalCount: 0 }
      mockedGetMarketplaceOffers.mockResolvedValue({ data: offersPage })

      const result = await marketplaceService.getMarketplaceOffers(
        { pageIndex: 2, pageSize: 10 },
        {
          seller: 42,
          onlyAffordable: true,
          offered: {
            itemType: ITEM_TYPE.OneHandedWeapon,
            itemRanks: [1, 2],
            item: { id: 'item_1', name: 'Sword' } as any,
            gold: [100, 500],
            heirloomPoints: 'None',
          },
          requested: {
            itemType: null,
            itemRanks: [],
            item: null,
            gold: 'Any',
            heirloomPoints: 'Any',
          },
        },
      )

      expect(result).toEqual(offersPage)
      expect(mockedGetMarketplaceOffers).toHaveBeenCalledWith({
        query: {
          page: 3,
          pageSize: 10,
          sellerId: 42,
          onlyAffordable: true,
          offeredItemId: 'item_1',
          offeredItemRanks: [1, 2],
          offeredItemType: ITEM_TYPE.OneHandedWeapon,
          offeredGold: '100,500',
          offeredHeirloomPoints: 'none',
          requestedItemId: undefined,
          requestedItemRanks: [],
          requestedItemType: undefined,
          requestedGold: undefined,
          requestedHeirloomPoints: undefined,
        },
      })
    })

    it('passes undefined for null/Any filter values', async () => {
      mockedGetMarketplaceOffers.mockResolvedValue({ data: { items: [], totalCount: 0 } })

      await marketplaceService.getMarketplaceOffers(
        { pageIndex: 0, pageSize: 5 },
        {
          seller: null,
          onlyAffordable: false,
          offered: marketplaceService.getDefaultMartetplaceSideFilterState(),
          requested: marketplaceService.getDefaultMartetplaceSideFilterState(),
        },
      )

      expect(mockedGetMarketplaceOffers).toHaveBeenCalledWith({
        query: expect.objectContaining({
          page: 1,
          sellerId: undefined,
          offeredItemId: undefined,
          offeredItemType: undefined,
          offeredGold: undefined,
          offeredHeirloomPoints: undefined,
          requestedGold: undefined,
          requestedHeirloomPoints: undefined,
        }),
      })
    })
  })

  describe('getMarketplaceOffersHistory', () => {
    it('converts 0-based pageIndex to 1-based page', async () => {
      const historyPage = { items: [], totalCount: 0 }
      mockedGetMarketplaceOffersHistory.mockResolvedValue({ data: historyPage })

      const result = await marketplaceService.getMarketplaceOffersHistory(
        { pageIndex: 0, pageSize: 20 },
        { seller: 7, buyer: 9 },
      )

      expect(result).toEqual(historyPage)
      expect(mockedGetMarketplaceOffersHistory).toHaveBeenCalledWith({
        query: { page: 1, pageSize: 20, sellerId: 7, buyerId: 9 },
      })
    })

    it('passes undefined for null seller/buyer', async () => {
      mockedGetMarketplaceOffersHistory.mockResolvedValue({ data: { items: [], totalCount: 0 } })

      await marketplaceService.getMarketplaceOffersHistory(
        { pageIndex: 1, pageSize: 10 },
        { seller: null, buyer: null },
      )

      expect(mockedGetMarketplaceOffersHistory).toHaveBeenCalledWith({
        query: { page: 2, pageSize: 10, sellerId: undefined, buyerId: undefined },
      })
    })
  })

  describe('createMarketplaceOffer', () => {
    it('posts offer payload and returns created offer', async () => {
      const offer = { id: 1 } as MarketplaceOffer
      mockedPostMarketplaceOffers.mockResolvedValue({ data: offer })

      const payload = {
        offer: { gold: 0, heirloomPoints: 0, userItemId: 11, itemId: null } as MarketplaceOfferAssetInput,
        request: { gold: 100, heirloomPoints: 0, userItemId: null, itemId: null } as MarketplaceOfferAssetInput,
      }

      await expect(marketplaceService.createMarketplaceOffer(payload)).resolves.toEqual(offer)
      expect(mockedPostMarketplaceOffers).toHaveBeenCalledWith({ body: payload })
    })
  })

  describe('deleteMarketplaceOffer', () => {
    it('calls delete with the correct offerId', () => {
      mockedDeleteMarketplaceOffersByOfferId.mockResolvedValue(null)
      marketplaceService.deleteMarketplaceOffer(55)
      expect(mockedDeleteMarketplaceOffersByOfferId).toHaveBeenCalledWith({ path: { offerId: 55 } })
    })
  })

  describe('acceptMarketplaceOffer', () => {
    it('calls accept with the correct offerId', () => {
      mockedPostMarketplaceOffersByOfferIdAccept.mockResolvedValue(null)
      marketplaceService.acceptMarketplaceOffer(99)
      expect(mockedPostMarketplaceOffersByOfferIdAccept).toHaveBeenCalledWith({ path: { offerId: 99 } })
    })
  })

  describe('canTradeItem', () => {
    it('returns false for Banner', () => {
      expect(marketplaceService.canTradeItem(ITEM_TYPE.Banner)).toBe(false)
    })

    it.each([
      ITEM_TYPE.OneHandedWeapon,
      ITEM_TYPE.TwoHandedWeapon,
      ITEM_TYPE.Shield,
      ITEM_TYPE.HeadArmor,
    ])('returns true for %s', (itemType) => {
      expect(marketplaceService.canTradeItem(itemType)).toBe(true)
    })
  })

  describe('canOfferUserItem', () => {
    const validMeta = { userItemId: 1, isBroken: false, isPersonal: false, clanArmoryLender: null }

    it('returns true when item is tradeable and meta has no restrictions', () => {
      expect(marketplaceService.canOfferUserItem(ITEM_TYPE.OneHandedWeapon, validMeta)).toBe(true)
    })

    it('returns false when item is broken', () => {
      expect(marketplaceService.canOfferUserItem(ITEM_TYPE.OneHandedWeapon, { ...validMeta, isBroken: true })).toBe(false)
    })

    it('returns false when item is personal', () => {
      expect(marketplaceService.canOfferUserItem(ITEM_TYPE.OneHandedWeapon, { ...validMeta, isPersonal: true })).toBe(false)
    })

    it('returns false when item is in clan armory', () => {
      expect(marketplaceService.canOfferUserItem(ITEM_TYPE.OneHandedWeapon, { ...validMeta, clanArmoryLender: { user: { id: 5 } } as any })).toBe(false)
    })

    it('returns false for Banner regardless of meta', () => {
      expect(marketplaceService.canOfferUserItem(ITEM_TYPE.Banner, validMeta)).toBe(false)
    })
  })

  describe('canAcceptOffer', () => {
    const makeOffer = (overrides: PartialDeep<MarketplaceOffer> = {}): MarketplaceOffer => ({
      id: 1,
      seller: { id: 10 } as any,
      createdAt: new Date(),
      goldFee: 0,
      offer: { side: MARKETPLACE_ASSET_SIDE.Offered, gold: 0, heirloomPoints: 0, item: null },
      request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 100, heirloomPoints: 0, item: null },
      ...overrides,
    } as MarketplaceOffer)

    const user: User = { id: 99, gold: 1000, heirloomPoints: 5 } as User

    it('returns true when all conditions are satisfied and no item is requested', () => {
      expect(marketplaceService.canAcceptOffer(makeOffer(), user, [])).toBe(true)
    })

    it('returns false when buyer is the seller', () => {
      const offer = makeOffer({ seller: { id: 99 } as any })
      expect(marketplaceService.canAcceptOffer(offer, user, [])).toBe(false)
    })

    it('returns false when user has insufficient gold', () => {
      const offer = makeOffer({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 9999, heirloomPoints: 0, item: null } })
      expect(marketplaceService.canAcceptOffer(offer, user, [])).toBe(false)
    })

    it('returns false when user has insufficient heirloom points', () => {
      const offer = makeOffer({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 100, item: null } })
      expect(marketplaceService.canAcceptOffer(offer, user, [])).toBe(false)
    })

    it('returns false when requested item is not in userItems', () => {
      const offer = makeOffer({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 0, item: { id: 'item_x' } as any } })
      const userItems: UserItem[] = [{ id: 1, item: { id: 'item_y', type: ITEM_TYPE.Shield } } as any]
      expect(marketplaceService.canAcceptOffer(offer, user, userItems)).toBe(false)
    })

    it('returns true when requested item is in userItems and is tradeable', () => {
      const offer = makeOffer({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 0, item: { id: 'item_x' } as any } })
      const userItems: UserItem[] = [{
        id: 1,
        item: { id: 'item_x', type: ITEM_TYPE.Shield },
        isBroken: false,
        isPersonal: false,
        clanArmoryLender: null,
      } as any]
      expect(marketplaceService.canAcceptOffer(offer, user, userItems)).toBe(true)
    })

    it('returns false when requested item is in userItems but is broken', () => {
      const offer = makeOffer({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 0, item: { id: 'item_x' } as any } })
      const userItems: UserItem[] = [{
        id: 1,
        item: { id: 'item_x', type: ITEM_TYPE.Shield },
        isBroken: true,
        isPersonal: false,
        clanArmoryLender: null,
      } as any]
      expect(marketplaceService.canAcceptOffer(offer, user, userItems)).toBe(false)
    })
  })

  describe('calculateFixedListingFee', () => {
    it('returns listingFeePerDay * offerDurationDays (50 * 7 = 350)', () => {
      expect(marketplaceService.calculateFixedListingFee()).toBe(350)
    })
  })

  describe('calculateGoldCommissionFee', () => {
    it('returns floor of gold * feePercent / 100', () => {
      expect(marketplaceService.calculateGoldCommissionFee(1000)).toBe(50)
      expect(marketplaceService.calculateGoldCommissionFee(99)).toBe(4)
      expect(marketplaceService.calculateGoldCommissionFee(0)).toBe(0)
    })
  })

  describe('calculateMaxOfferGold', () => {
    it('returns max gold user can offer after listing fee and commission', () => {
      // floor((1000 - 350) / (1 + 5/100)) = floor(650 / 1.05) = floor(619.047...) = 619
      expect(marketplaceService.calculateMaxOfferGold(1000)).toBe(619)
    })

    it('returns 0 when user gold equals listing fee', () => {
      expect(marketplaceService.calculateMaxOfferGold(350)).toBe(0)
    })
  })

  describe('calculateMaxRequestGold', () => {
    it('returns max gold user can request given listing fee and fee percent', () => {
      // floor((1000 - 350) / (5/100)) = floor(650 / 0.05) = 13000
      expect(marketplaceService.calculateMaxRequestGold(1000)).toBe(13000)
    })

    it('returns 0 when user gold equals listing fee', () => {
      expect(marketplaceService.calculateMaxRequestGold(350)).toBe(0)
    })
  })
})
