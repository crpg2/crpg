import type { ItemType, SelectedItem } from '~/models/item'
import type { UserPublic } from '~/models/user'

export const MARKETPLACE_OFFER_STATUS = {
  Active: 'Active',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  Expired: 'Expired',
} as const

export type MarketplaceOfferStatus = (typeof MARKETPLACE_OFFER_STATUS)[keyof typeof MARKETPLACE_OFFER_STATUS]

export const MARKETPLACE_ASSET_SIDE = {
  Offered: 'Offered',
  Requested: 'Requested',
} as const

export type MarketplaceAssetSide = (typeof MARKETPLACE_ASSET_SIDE)[keyof typeof MARKETPLACE_ASSET_SIDE]

export interface MarketplaceOffer {
  id: number
  seller: UserPublic
  createdAt: Date
  offer: MarketplaceOfferAsset
  request: MarketplaceOfferAsset
  goldFee: number
}

export interface MarketplaceOfferAsset {
  side: MarketplaceAssetSide
  gold: number
  heirloomPoints: number
  item: SelectedItem | null
}

export interface MarketplaceOfferAssetInput {
  gold: number
  heirloomPoints: number
  userItemId: number | null
  itemId: string | null
}

export interface MarketplaceOffersPage {
  items: MarketplaceOffer[]
  totalCount: number
}

export interface MartetplaceFilter {
  offered: MartetplaceSideFilter
  requested: MartetplaceSideFilter
  seller: number | null
  onlyAffordable: boolean
}

export type MartetplaceCurrencyFilter = 'Any' | 'None' | [number, number]

export interface MartetplaceSideFilter {
  itemType: ItemType | null
  itemRanks: number[]
  item: SelectedItem | null
  gold: MartetplaceCurrencyFilter
  heirloomPoints: MartetplaceCurrencyFilter
}

export interface MarketplaceOfferHistorySide {
  gold: number
  heirloomPoints: number
  item?: SelectedItem | null
}

export interface MarketplaceOfferHistory {
  id: number
  seller: UserPublic
  buyer: UserPublic
  goldFee: number
  acceptedAt: Date
  offer: MarketplaceOfferAsset
  request: MarketplaceOfferAsset
}

export interface MarketplaceOffersHistoryPage {
  items: MarketplaceOfferHistory[]
  totalCount: number
}

export interface MartetplaceHistoryFilter {
  seller: number | null
  buyer: number | null
}
