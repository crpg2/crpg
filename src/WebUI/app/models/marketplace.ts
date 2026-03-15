export const MARKETPLACE_ASSET_TYPE = {
  Gold: 'Gold',
  HeirloomPoints: 'HeirloomPoints',
  Item: 'Item',
} as const

export type MarketplaceAssetType = (typeof MARKETPLACE_ASSET_TYPE)[keyof typeof MARKETPLACE_ASSET_TYPE]

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

export interface MarketplaceOfferAsset {
  id: number
  side: MarketplaceAssetSide
  type: MarketplaceAssetType
  amount: number
  itemId: string | null
  userItemId: number | null
  item?: {
    id: string
    name: string
    rank: number
  } | null
}

export interface MarketplaceOffer {
  id: number
  sellerUserId: number
  buyerUserId: number | null
  status: MarketplaceOfferStatus
  createdAt: string
  updatedAt: string
  expiresAt: string
  closedAt: string | null
  assets: MarketplaceOfferAsset[]
}

export interface MarketplaceOfferAssetInput {
  type: MarketplaceAssetType
  amount: number
  userItemId?: number
  itemId?: string
}

export interface CreateMarketplaceOfferPayload {
  offeredAssets: MarketplaceOfferAssetInput[]
  requestedAssets: MarketplaceOfferAssetInput[]
}
