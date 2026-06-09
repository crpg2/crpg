import type { PartialDeep } from 'type-fest'

import { describe, expect, it, vi } from 'vitest'

import type { MarketplaceListing, MarketplaceListingAssetInput } from '~/models/marketplace'
import type { User, UserItem } from '~/models/user'

import { ITEM_TYPE } from '~/models/item'
import { MARKETPLACE_ASSET_SIDE } from '~/models/marketplace'

import * as marketplaceService from '../marketplace-service'

const {
  mockedDeleteMarketplaceListingsByListingId,
  mockedGetMarketplaceListings,
  mockedGetMarketplaceListingsHistory,
  mockedPostMarketplaceListings,
  mockedPostMarketplaceListingsByListingIdAccept,
} = vi.hoisted(() => ({
  mockedDeleteMarketplaceListingsByListingId: vi.fn(),
  mockedGetMarketplaceListings: vi.fn(),
  mockedGetMarketplaceListingsHistory: vi.fn(),
  mockedPostMarketplaceListings: vi.fn(),
  mockedPostMarketplaceListingsByListingIdAccept: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  deleteMarketplaceListingsByListingId: mockedDeleteMarketplaceListingsByListingId,
  getMarketplaceListings: mockedGetMarketplaceListings,
  getMarketplaceListingsHistory: mockedGetMarketplaceListingsHistory,
  postMarketplaceListings: mockedPostMarketplaceListings,
  postMarketplaceListingsByListingIdAccept: mockedPostMarketplaceListingsByListingIdAccept,
}))

vi.mock('~/api.config', () => ({
  unwrapData: (result: { data: unknown }) => result.data,
}))

describe('marketplace service', () => {
  describe('getMarketplaceListings', () => {
    it('converts 0-based pageIndex to 1-based page and passes all filter params', async () => {
      const listingsPage = { items: [], totalCount: 0 }
      mockedGetMarketplaceListings.mockResolvedValue({ data: listingsPage })

      const result = await marketplaceService.getMarketplaceListings(
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

      expect(result).toEqual(listingsPage)
      expect(mockedGetMarketplaceListings).toHaveBeenCalledWith({
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
      mockedGetMarketplaceListings.mockResolvedValue({ data: { items: [], totalCount: 0 } })

      await marketplaceService.getMarketplaceListings(
        { pageIndex: 0, pageSize: 5 },
        {
          seller: null,
          onlyAffordable: false,
          offered: marketplaceService.getDefaultMartetplaceSideFilterState(),
          requested: marketplaceService.getDefaultMartetplaceSideFilterState(),
        },
      )

      expect(mockedGetMarketplaceListings).toHaveBeenCalledWith({
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

  describe('getMarketplaceListingsHistory', () => {
    it('converts 0-based pageIndex to 1-based page', async () => {
      const historyPage = { items: [], totalCount: 0 }
      mockedGetMarketplaceListingsHistory.mockResolvedValue({ data: historyPage })

      const result = await marketplaceService.getMarketplaceListingsHistory(
        { pageIndex: 0, pageSize: 20 },
        { seller: 7, buyer: 9 },
      )

      expect(result).toEqual(historyPage)
      expect(mockedGetMarketplaceListingsHistory).toHaveBeenCalledWith({
        query: { page: 1, pageSize: 20, sellerId: 7, buyerId: 9 },
      })
    })

    it('passes undefined for null seller/buyer', async () => {
      mockedGetMarketplaceListingsHistory.mockResolvedValue({ data: { items: [], totalCount: 0 } })

      await marketplaceService.getMarketplaceListingsHistory(
        { pageIndex: 1, pageSize: 10 },
        { seller: null, buyer: null },
      )

      expect(mockedGetMarketplaceListingsHistory).toHaveBeenCalledWith({
        query: { page: 2, pageSize: 10, sellerId: undefined, buyerId: undefined },
      })
    })
  })

  describe('createMarketplaceListing', () => {
    it('posts listing payload and returns created listing', async () => {
      const listing = { id: 1 } as MarketplaceListing
      mockedPostMarketplaceListings.mockResolvedValue({ data: listing })

      const payload = {
        offer: { gold: 0, heirloomPoints: 0, userItemId: 11, itemId: null } as MarketplaceListingAssetInput,
        request: { gold: 100, heirloomPoints: 0, userItemId: null, itemId: null } as MarketplaceListingAssetInput,
      }

      await expect(marketplaceService.createMarketplaceListing(payload)).resolves.toEqual(listing)
      expect(mockedPostMarketplaceListings).toHaveBeenCalledWith({ body: payload })
    })
  })

  describe('deleteMarketplaceListing', () => {
    it('calls delete with the correct listingId', () => {
      mockedDeleteMarketplaceListingsByListingId.mockResolvedValue(null)
      marketplaceService.deleteMarketplaceListing(55)
      expect(mockedDeleteMarketplaceListingsByListingId).toHaveBeenCalledWith({ path: { listingId: 55 } })
    })
  })

  describe('acceptMarketplaceListing', () => {
    it('calls accept with the correct listingId', () => {
      mockedPostMarketplaceListingsByListingIdAccept.mockResolvedValue(null)
      marketplaceService.acceptMarketplaceListing(99)
      expect(mockedPostMarketplaceListingsByListingIdAccept).toHaveBeenCalledWith({ path: { listingId: 99 } })
    })
  })

  describe('canListingItem', () => {
    it('returns false for Banner', () => {
      expect(marketplaceService.canListingItem(ITEM_TYPE.Banner)).toBe(false)
    })

    it.each([
      ITEM_TYPE.OneHandedWeapon,
      ITEM_TYPE.TwoHandedWeapon,
      ITEM_TYPE.Shield,
      ITEM_TYPE.HeadArmor,
    ])('returns true for %s', (itemType) => {
      expect(marketplaceService.canListingItem(itemType)).toBe(true)
    })
  })

  describe('canListingUserItem', () => {
    const validMeta = { userItemId: 1, isBroken: false, isPersonal: false, clanArmoryLender: null }

    it('returns true when item is tradeable and meta has no restrictions', () => {
      expect(marketplaceService.canListingUserItem(ITEM_TYPE.OneHandedWeapon, validMeta)).toBe(true)
    })

    it('returns false when item is broken', () => {
      expect(marketplaceService.canListingUserItem(ITEM_TYPE.OneHandedWeapon, { ...validMeta, isBroken: true })).toBe(false)
    })

    it('returns false when item is personal', () => {
      expect(marketplaceService.canListingUserItem(ITEM_TYPE.OneHandedWeapon, { ...validMeta, isPersonal: true })).toBe(false)
    })

    it('returns false when item is in clan armory', () => {
      expect(marketplaceService.canListingUserItem(ITEM_TYPE.OneHandedWeapon, { ...validMeta, clanArmoryLender: { user: { id: 5 } } as any })).toBe(false)
    })

    it('returns false for Banner regardless of meta', () => {
      expect(marketplaceService.canListingUserItem(ITEM_TYPE.Banner, validMeta)).toBe(false)
    })
  })

  describe('canAcceptListing', () => {
    const makeListing = (overrides: PartialDeep<MarketplaceListing> = {}): MarketplaceListing => ({
      id: 1,
      seller: { id: 10 } as any,
      createdAt: new Date(),
      goldFee: 0,
      offer: { side: MARKETPLACE_ASSET_SIDE.Offered, gold: 0, heirloomPoints: 0, item: null },
      request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 100, heirloomPoints: 0, item: null },
      ...overrides,
    } as MarketplaceListing)

    const user: User = { id: 99, gold: 1000, heirloomPoints: 5 } as User

    it('returns true when all conditions are satisfied and no item is requested', () => {
      expect(marketplaceService.canAcceptListing(makeListing(), user, [])).toBe(true)
    })

    it('returns false when buyer is the seller', () => {
      const listing = makeListing({ seller: { id: 99 } as any })
      expect(marketplaceService.canAcceptListing(listing, user, [])).toBe(false)
    })

    it('returns false when user has insufficient gold', () => {
      const listing = makeListing({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 9999, heirloomPoints: 0, item: null } })
      expect(marketplaceService.canAcceptListing(listing, user, [])).toBe(false)
    })

    it('returns false when user has insufficient heirloom points', () => {
      const listing = makeListing({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 100, item: null } })
      expect(marketplaceService.canAcceptListing(listing, user, [])).toBe(false)
    })

    it('returns false when requested item is not in userItems', () => {
      const listing = makeListing({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 0, item: { id: 'item_x' } as any } })
      const userItems: UserItem[] = [{ id: 1, item: { id: 'item_y', type: ITEM_TYPE.Shield } } as any]
      expect(marketplaceService.canAcceptListing(listing, user, userItems)).toBe(false)
    })

    it('returns true when requested item is in userItems and is tradeable', () => {
      const listing = makeListing({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 0, item: { id: 'item_x' } as any } })
      const userItems: UserItem[] = [{
        id: 1,
        item: { id: 'item_x', type: ITEM_TYPE.Shield },
        isBroken: false,
        isPersonal: false,
        clanArmoryLender: null,
      } as any]
      expect(marketplaceService.canAcceptListing(listing, user, userItems)).toBe(true)
    })

    it('returns false when requested item is in userItems but is broken', () => {
      const listing = makeListing({ request: { side: MARKETPLACE_ASSET_SIDE.Requested, gold: 0, heirloomPoints: 0, item: { id: 'item_x' } as any } })
      const userItems: UserItem[] = [{
        id: 1,
        item: { id: 'item_x', type: ITEM_TYPE.Shield },
        isBroken: true,
        isPersonal: false,
        clanArmoryLender: null,
      } as any]
      expect(marketplaceService.canAcceptListing(listing, user, userItems)).toBe(false)
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
