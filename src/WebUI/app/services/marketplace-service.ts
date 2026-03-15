import type { PaginationState } from '@tanstack/vue-table'

import {
  getMarketplaceOffers as _getMarketplaceOffers,
  getMarketplaceOffersHistory as _getMarketplaceOffersHistory,
  deleteMarketplaceOffersByOfferId,
  postMarketplaceOffers,
  postMarketplaceOffersByOfferIdAccept,
} from '#api/sdk.gen'
import { marketplaceGoldFeePercent, marketplaceListingFeePerDay, marketplaceOfferDurationDays } from '~root/data/constants.json'

import type { ItemType, UserItemMeta } from '~/models/item'
import type { MarketplaceOffer, MarketplaceOfferAssetInput, MarketplaceOffersHistoryPage, MarketplaceOffersPage, MartetplaceCurrencyFilter, MartetplaceFilter, MartetplaceHistoryFilter, MartetplaceSideFilter } from '~/models/marketplace'
import type { User, UserItem } from '~/models/user'

import { unwrapData } from '~/api.config'
import { ITEM_TYPE } from '~/models/item'

const toCurrencyQuery = (filter: MartetplaceCurrencyFilter): string | undefined => {
  if (filter === 'Any') {
    return undefined
  }

  if (filter === 'None') {
    return 'none'
  }

  return `${filter[0]},${filter[1]}`
}

export const getMarketplaceOffers = async (
  pagination: PaginationState,
  filter: MartetplaceFilter,
): Promise<MarketplaceOffersPage> => {
  const { pageIndex, pageSize } = pagination
  const { seller, onlyAffordable, offered, requested } = filter

  return unwrapData(await _getMarketplaceOffers({ query: {
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

export const getMarketplaceOffersHistory = async (
  pagination: PaginationState,
  filter: MartetplaceHistoryFilter,
): Promise<MarketplaceOffersHistoryPage> => {
  const { pageIndex, pageSize } = pagination
  const { seller, buyer } = filter

  return unwrapData(await _getMarketplaceOffersHistory({ query: {
    page: pageIndex + 1, // tanstack table is 0-based, API is 1-based
    pageSize,
    sellerId: seller ?? undefined,
    buyerId: buyer ?? undefined,
  } }))
}

export const createMarketplaceOffer = async (payload: { offer: MarketplaceOfferAssetInput, request: MarketplaceOfferAssetInput }): Promise<MarketplaceOffer> =>
  unwrapData(await postMarketplaceOffers({ body: payload }))

export const deleteMarketplaceOffer = (offerId: number) => deleteMarketplaceOffersByOfferId({ path: { offerId } })

export const acceptMarketplaceOffer = (offerId: number) => postMarketplaceOffersByOfferIdAccept({ path: { offerId } })

export const canTradeItem = (itemType: ItemType): boolean =>
  itemType !== ITEM_TYPE.Banner

export const canOfferUserItem = (itemType: ItemType, userItemMeta: UserItemMeta): boolean =>
  !userItemMeta.isBroken
  && !userItemMeta.isPersonal
  && !userItemMeta.clanArmoryLender
  && canTradeItem(itemType)

export const canAcceptOffer = (offer: MarketplaceOffer, user: User, userItems: UserItem[]) => {
  return offer.seller.id !== user.id
    && offer.request.gold <= user.gold
    && offer.request.heirloomPoints <= user.heirloomPoints
    && (offer.request.item === null
      || userItems.some(ui => ui.item.id === offer.request.item!.id
        && canOfferUserItem(ui.item.type, {
          userItemId: ui.id,
          isBroken: ui.isBroken,
          isPersonal: ui.isPersonal,
          clanArmoryLender: ui.clanArmoryLender,
        })))
}

export function getDefaultMartetplaceSideFilterState(): MartetplaceSideFilter {
  return {
    itemType: null,
    itemRanks: [],
    item: null,
    gold: 'Any',
    heirloomPoints: 'Any',
  }
}

export const calculateFixedListingFee = () => marketplaceListingFeePerDay * marketplaceOfferDurationDays

export const calculateGoldCommissionFee = (gold: number) => Math.floor(gold * marketplaceGoldFeePercent / 100)

export const calculateMaxOfferGold = (userGold: number) =>
  Math.floor((userGold - calculateFixedListingFee()) / (1 + marketplaceGoldFeePercent / 100))

export const calculateMaxRequestGold = (userGold: number) =>
  Math.floor((userGold - calculateFixedListingFee()) / (marketplaceGoldFeePercent / 100))
