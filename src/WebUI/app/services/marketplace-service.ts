import type { PaginationState } from '@tanstack/vue-table'

import {
  getMarketplaceListings as _getMarketplaceListings,
  getMarketplaceListingsHistory as _getMarketplaceListingsHistory,
  deleteMarketplaceListingsByListingId,
  postMarketplaceListings,
  postMarketplaceListingsByListingIdAccept,
} from '#api/sdk.gen'
import { marketplaceGoldFeePercent, marketplaceListingDurationDays, marketplaceListingFeePerDay } from '~root/data/constants.json'

import type { ItemType, UserItemMeta } from '~/models/item'
import type { MarketplaceListing, MarketplaceListingAssetInput, MarketplaceListingsHistoryPage, MarketplaceListingsPage, MartetplaceListingsCurrencyFilter, MartetplaceListingsFilter, MartetplaceListingsHistoryFilter, MartetplaceListingsSideFilter } from '~/models/marketplace'
import type { User, UserItem } from '~/models/user'

import { unwrapData } from '~/api.config'
import { ITEM_TYPE } from '~/models/item'

const toCurrencyQuery = (filter: MartetplaceListingsCurrencyFilter): string | undefined => {
  if (filter === 'Any') {
    return undefined
  }

  if (filter === 'None') {
    return 'none'
  }

  return `${filter[0]},${filter[1]}`
}

export const getMarketplaceListings = async (
  pagination: PaginationState,
  filter: MartetplaceListingsFilter,
): Promise<MarketplaceListingsPage> => {
  const { pageIndex, pageSize } = pagination
  const { seller, onlyAffordable, offered, requested } = filter

  return unwrapData(await _getMarketplaceListings({ query: {
    page: pageIndex + 1, // tanstack table is 0-based, API is 1-based
    pageSize,
    sellerId: seller ?? undefined,
    onlyAffordable,
    offeredItemId: offered.item?.id ?? undefined,
    offeredItemRanks: offered.itemRanks,
    offeredItemType: offered.itemType ?? undefined,
    offeredGold: toCurrencyQuery(offered.gold),
    offeredHeirloomPoints: toCurrencyQuery(offered.heirloomPoints),
    requestedItemId: requested.item?.id ?? undefined,
    requestedItemRanks: requested.itemRanks,
    requestedItemType: requested.itemType ?? undefined,
    requestedGold: toCurrencyQuery(requested.gold),
    requestedHeirloomPoints: toCurrencyQuery(requested.heirloomPoints),
  } }))
}

export const getMarketplaceListingsHistory = async (
  pagination: PaginationState,
  filter: MartetplaceListingsHistoryFilter,
): Promise<MarketplaceListingsHistoryPage> => {
  const { pageIndex, pageSize } = pagination
  const { seller, buyer } = filter

  return unwrapData(await _getMarketplaceListingsHistory({ query: {
    page: pageIndex + 1, // tanstack table is 0-based, API is 1-based
    pageSize,
    sellerId: seller ?? undefined,
    buyerId: buyer ?? undefined,
  } }))
}

export const createMarketplaceListing = async (payload: { offer: MarketplaceListingAssetInput, request: MarketplaceListingAssetInput }): Promise<MarketplaceListing> =>
  unwrapData(await postMarketplaceListings({ body: payload }))

export const deleteMarketplaceListing = (listingId: number) => deleteMarketplaceListingsByListingId({ path: { listingId } })

export const acceptMarketplaceListing = (listingId: number) => postMarketplaceListingsByListingIdAccept({ path: { listingId } })

export const canListingItem = (itemType: ItemType): boolean =>
  itemType !== ITEM_TYPE.Banner

export const canListingUserItem = (itemType: ItemType, userItemMeta: UserItemMeta): boolean =>
  !userItemMeta.isBroken
  && !userItemMeta.isPersonal
  && !userItemMeta.clanArmoryLender
  && canListingItem(itemType)

export const canAcceptListing = (listing: MarketplaceListing, user: User, userItems: UserItem[]) => {
  return listing.seller.id !== user.id
    && listing.request.gold <= user.gold
    && listing.request.heirloomPoints <= user.heirloomPoints
    && (listing.request.item === null
      || userItems.some(ui => ui.item.id === listing.request.item!.id
        && canListingUserItem(ui.item.type, {
          userItemId: ui.id,
          isBroken: ui.isBroken,
          isPersonal: ui.isPersonal,
          clanArmoryLender: ui.clanArmoryLender,
        })))
}

export function getDefaultMartetplaceSideFilterState(): MartetplaceListingsSideFilter {
  return {
    itemType: null,
    itemRanks: [],
    item: null,
    gold: 'Any',
    heirloomPoints: 'Any',
  }
}

export const calculateFixedListingFee = () => marketplaceListingFeePerDay * marketplaceListingDurationDays

export const calculateGoldCommissionFee = (gold: number) => Math.floor(gold * marketplaceGoldFeePercent / 100)

export const calculateMaxOfferGold = (userGold: number) =>
  Math.floor((userGold - calculateFixedListingFee()) / (1 + marketplaceGoldFeePercent / 100))

export const calculateMaxRequestGold = (userGold: number) =>
  Math.floor((userGold - calculateFixedListingFee()) / (marketplaceGoldFeePercent / 100))
