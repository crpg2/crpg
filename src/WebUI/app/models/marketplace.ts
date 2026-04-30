import type { ValueOf } from 'type-fest'

import type { ItemType, SelectedItem } from '~/models/item'
import type { UserPublic } from '~/models/user'

export const MARKETPLACE_LISTING_STATUS = {
  Active: 'Active',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  Expired: 'Expired',
} as const

export type MarketplaceListingStatus = ValueOf<typeof MARKETPLACE_LISTING_STATUS>

export const MARKETPLACE_ASSET_SIDE = {
  Offered: 'Offered',
  Requested: 'Requested',
} as const

export type MarketplaceAssetSide = ValueOf<typeof MARKETPLACE_ASSET_SIDE>

export interface MarketplaceListing {
  id: number
  seller: UserPublic
  createdAt: Date
  offer: MarketplaceListingAsset
  request: MarketplaceListingAsset
  goldFee: number
}

export interface MarketplaceListingAsset {
  side: MarketplaceAssetSide
  gold: number
  heirloomPoints: number
  item: SelectedItem | null
}

export interface MarketplaceListingAssetInput {
  gold: number
  heirloomPoints: number
  userItemId: number | null
  itemId: string | null
}

export interface MarketplaceListingsPage {
  items: MarketplaceListing[]
  totalCount: number
}

export interface MartetplaceListingsFilter {
  offered: MartetplaceListingsSideFilter
  requested: MartetplaceListingsSideFilter
  seller: number | null
  onlyAffordable: boolean
}

export interface MartetplaceListingsSideFilter {
  itemType: ItemType | null
  itemRanks: number[]
  item: SelectedItem | null
  gold: MartetplaceListingsCurrencyFilter
  heirloomPoints: MartetplaceListingsCurrencyFilter
}

export type MartetplaceListingsCurrencyFilter = 'Any' | 'None' | [number, number]

export interface MarketplaceListingHistory {
  id: number
  seller: UserPublic
  buyer: UserPublic
  goldFee: number
  acceptedAt: Date
  offer: MarketplaceListingAsset
  request: MarketplaceListingAsset
}

export interface MarketplaceListingsHistoryPage {
  items: MarketplaceListingHistory[]
  totalCount: number
}

export interface MartetplaceListingsHistoryFilter {
  seller: number | null
  buyer: number | null
}
