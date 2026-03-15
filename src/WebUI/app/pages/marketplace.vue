<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/vue-table'

import { getPaginationRowModel } from '@tanstack/vue-table'
import { UserMedia } from '#components'

import type { Item } from '~/models/item'
import type { UserPublic } from '~/models/user'

const table = useTemplateRef('table')
const { getInitialPaginationState, pagination } = usePagination()
const columnFilters = ref<ColumnFiltersState>([])
const searchModel = ref<string | undefined>(undefined)

export interface MarketplaceOfferAsset {
  id: number
  side: 'Offered' | 'Requested'
  type: 'Gold' | 'HeirloomPoints' | 'Item'
  amount: number // for gold, HeirloomPoints
  userItemId: number | null // for Offered
  itemId: string | null // for Requested
  item: Item | null
}

export interface MarketplaceOffer {
  id: number
  user: UserPublic
  // status: MarketplaceOfferStatus
  createdAt: string
  updatedAt: string
  expiresAt: string
  // closedAt: string | null
  assets: MarketplaceOfferAsset[]
  // offered: MarketplaceOfferAsset
  // requested: MarketplaceOfferAsset
}

const marketplaceOffers = ref<MarketplaceOffer[]>([
  {
    id: 1,
    user: {
      id: 1,
      name: 'PlayerOne',
      clanMembership: null,
      avatar: '',
      platform: 'Steam',
      region: 'Eu',
      platformUserId: '123456789',
    },
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
    expiresAt: new Date(Date.now() + 3600 * 1000).toISOString(),
    assets: [
      {
        id: 1,
        side: 'Offered',
        type: 'Gold',
        amount: 100000,
        userItemId: null,
        itemId: null,
        item: null,
      },
      {
        id: 1,
        side: 'Offered',
        type: 'Gold',
        amount: 100000,
        userItemId: 1,
        itemId: null,
        item: {},
      },
      {
        id: 2,
        side: 'Requested',
        type: 'HeirloomPoints',
        amount: 33,
        userItemId: null,
        itemId: null,
        item: null,
      },
    ],
  },
])

const columns: TableColumn<MarketplaceOffer>[] = [
  {
    accessorKey: 'user.name',
    id: 'user_name',
    // header: ({ column }) => h(UInput, {
    //   'icon': 'crpg:search',
    //   'placeholder': t('clan.table.column.name'),
    //   'modelValue': column.getFilterValue() as string,
    //   'onUpdate:modelValue': column.setFilterValue,
    // }),
    cell: ({ row }) => h(UserMedia, {
      user: row.original.user,
      // isSelf: checkIsSelfMember(row.original),
      hiddenClan: true,
    }),
  },
  {
    accessorKey: 'assets',
    cell: ({ row }) => h('div', { class: 'space-y-2' }, [
      h('div', [
        h('span', { class: 'font-semibold' }, 'Give: '),
        row.original.assets.filter(asset => asset.side === 'Offered').map((asset) => {
          if (asset.type === 'Gold') {
            return `${asset.amount} gold`
          }
          if (asset.type === 'HeirloomPoints') {
            return `${asset.amount} loom`
          }
          if (asset.type === 'Item') {
            const itemName = asset.item?.name ?? asset.itemId ?? 'item'
            return `${asset.amount}x ${itemName}`
          }
          return null
        }).filter(Boolean).join(', ') || 'none',
      ]),
      h('div', [
        h('span', { class: 'font-semibold' }, 'Want: '),
        row.original.assets.filter(asset => asset.side === 'Requested').map((asset) => {
          if (asset.type === 'Gold') {
            return `${asset.amount} gold`
          }
          if (asset.type === 'HeirloomPoints') {
            return `${asset.amount} loom`
          }
          if (asset.type === 'Item') {
            const itemName = asset.item?.name ?? asset.itemId ?? 'item'
            return `${asset.amount}x ${itemName}`
          }
          return null
        }).filter(Boolean).join(', ') || 'none',
      ]),
    ]),
  },
]
</script>

<template>
  <UContainer
    class="space-y-8 py-12"
  >
    <div class="mx-auto max-w-3xl space-y-4">
      <!--
      :loading="loadingClanMembers"
      -->

      <UTable
        ref="table"
        v-model:pagination="pagination"
        v-model:column-filters="columnFilters"
        v-model:global-filter="searchModel"
        class="relative rounded-md border border-muted"
        :data="marketplaceOffers"
        :columns
        :initial-state="{
          pagination: getInitialPaginationState(),
        }"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
      >
        <template #empty>
          <UiResultNotFound />
        </template>
      </UTable>

      <UiGridPagination v-if="table?.tableApi" :table-api="toRef(() => table!.tableApi)" />
    </div>
  </UContainer>
</template>

<!-- <script setup lang="ts">
import type { MarketplaceOffer } from '~/models/marketplace'

import { useUser } from '~/composables/user/use-user'
import { useUserItemsProvider } from '~/composables/user/use-user-items'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { useAsyncDataCustom } from '~/composables/utils/use-async-data-custom'
import { MARKETPLACE_ASSET_SIDE, MARKETPLACE_ASSET_TYPE, MARKETPLACE_OFFER_STATUS } from '~/models/marketplace'
import { SomeRole } from '~/models/role'
import { getItems } from '~/services/item-service'
import { acceptMarketplaceOffer, cancelMarketplaceOffer, createMarketplaceOffer, getMarketplaceOffers, getSelfMarketplaceOffers } from '~/services/marketplace-service'

definePageMeta({
  roles: SomeRole,
})

const { n } = useI18n()
const { user, fetchUser } = useUser()
const { data: userItems } = useUserItemsProvider()

const { data: catalogItems } = useAsyncDataCustom(
  () => ['marketplace', 'catalog-items'],
  () => getItems(),
  { default: () => [] },
)

const { data: offers, refresh: refreshOffers, pending: loadingOffers } = useAsyncDataCustom(
  () => ['marketplace', 'offers'],
  () => getMarketplaceOffers(),
  { default: () => [] },
)

const { data: selfOffers, refresh: refreshSelfOffers, pending: loadingSelfOffers } = useAsyncDataCustom(
  () => ['marketplace', 'self-offers'],
  () => getSelfMarketplaceOffers(),
  { default: () => [] },
)

const offeredGoldAmount = ref<number>(300000)
const offeredHeirloomAmount = ref<number>(0)
const offeredUserItemId = ref<number | undefined>()

const requestedGoldAmount = ref<number>(0)
const requestedHeirloomAmount = ref<number>(0)
const requestedItemId = ref<string | undefined>()
const requestedItemAmount = ref<number>(1)

const activeOffersCount = computed(() =>
  selfOffers.value.filter(offer => offer.status === MARKETPLACE_OFFER_STATUS.Active).length,
)
const canCreateOffer = computed(() => activeOffersCount.value < 5)

const offeredItemOptions = computed(() => userItems.value.map(userItem => ({
  label: `${userItem.item.name} (+${userItem.item.rank}) #${userItem.id}`,
  value: userItem.id,
})))

const requestedItemOptions = computed(() => catalogItems.value.map(item => ({
  label: `${item.name} (+${item.rank})`,
  value: item.id,
})))

const offeredSelectedItemLabel = computed(() =>
  offeredItemOptions.value.find(option => option.value === offeredUserItemId.value)?.label ?? null,
)

const requestedSelectedItemLabel = computed(() =>
  requestedItemOptions.value.find(option => option.value === requestedItemId.value)?.label ?? null,
)

const createOfferPreview = computed(() => ({
  give: [
    ...(offeredGoldAmount.value > 0 ? [`${n(offeredGoldAmount.value)} gold`] : []),
    ...(offeredHeirloomAmount.value > 0 ? [`${offeredHeirloomAmount.value} loom`] : []),
    ...(offeredSelectedItemLabel.value ? [offeredSelectedItemLabel.value] : []),
  ],
  want: [
    ...(requestedGoldAmount.value > 0 ? [`${n(requestedGoldAmount.value)} gold`] : []),
    ...(requestedHeirloomAmount.value > 0 ? [`${requestedHeirloomAmount.value} loom`] : []),
    ...(requestedSelectedItemLabel.value ? [`${requestedItemAmount.value}x ${requestedSelectedItemLabel.value}`] : []),
  ],
}))

const isCreateDisabled = computed(() => {
  if (!canCreateOffer.value) {
    return true
  }

  const hasOfferedAssets = offeredGoldAmount.value > 0 || offeredHeirloomAmount.value > 0 || !!offeredUserItemId.value
  const hasRequestedAssets = requestedGoldAmount.value > 0 || requestedHeirloomAmount.value > 0 || !!requestedItemId.value

  if (!hasOfferedAssets || !hasRequestedAssets) {
    return true
  }

  if (requestedItemId.value && requestedItemAmount.value <= 0) {
    return true
  }

  return false
})

const [onCreateOffer, creatingOffer] = useAsyncCallback(async () => {
  const offeredAssets = [
    ...(offeredGoldAmount.value > 0
      ? [{ type: MARKETPLACE_ASSET_TYPE.Gold, amount: offeredGoldAmount.value }]
      : []),
    ...(offeredHeirloomAmount.value > 0
      ? [{ type: MARKETPLACE_ASSET_TYPE.HeirloomPoints, amount: offeredHeirloomAmount.value }]
      : []),
    ...(offeredUserItemId.value
      ? [{ type: MARKETPLACE_ASSET_TYPE.Item, amount: 1, userItemId: offeredUserItemId.value }]
      : []),
  ]

  const requestedAssets = [
    ...(requestedGoldAmount.value > 0
      ? [{ type: MARKETPLACE_ASSET_TYPE.Gold, amount: requestedGoldAmount.value }]
      : []),
    ...(requestedHeirloomAmount.value > 0
      ? [{ type: MARKETPLACE_ASSET_TYPE.HeirloomPoints, amount: requestedHeirloomAmount.value }]
      : []),
    ...(requestedItemId.value
      ? [{ type: MARKETPLACE_ASSET_TYPE.Item, amount: requestedItemAmount.value, itemId: requestedItemId.value }]
      : []),
  ]

  await createMarketplaceOffer({
    offeredAssets,
    requestedAssets,
  })

  await Promise.all([
    refreshOffers(),
    refreshSelfOffers(),
    fetchUser(),
  ])
}, {
  successMessage: 'Offer created',
})

const [onCancelOffer, cancellingOffer] = useAsyncCallback(async (offerId: number) => {
  await cancelMarketplaceOffer(offerId)
  await Promise.all([
    refreshOffers(),
    refreshSelfOffers(),
    fetchUser(),
  ])
}, {
  successMessage: 'Offer cancelled',
})

const [onAcceptOffer, acceptingOffer] = useAsyncCallback(async (offerId: number) => {
  await acceptMarketplaceOffer(offerId)
  await Promise.all([
    refreshOffers(),
    refreshSelfOffers(),
    fetchUser(),
  ])
}, {
  successMessage: 'Offer accepted',
})

const submitting = computed(() => creatingOffer.value || cancellingOffer.value || acceptingOffer.value)

function summarizeAssets(offer: MarketplaceOffer, side: 'Offered' | 'Requested') {
  const assets = offer.assets.filter(asset => asset.side === side)

  if (!assets.length) {
    return 'none'
  }

  return assets.map((asset) => {
    if (asset.type === MARKETPLACE_ASSET_TYPE.Gold) {
      return `${n(asset.amount)} gold`
    }

    if (asset.type === MARKETPLACE_ASSET_TYPE.HeirloomPoints) {
      return `${asset.amount} loom`
    }

    const itemName = asset.item?.name ?? asset.itemId ?? 'item'
    return `${asset.amount}x ${itemName}`
  }).join(', ')
}

function isSelfOffer(offer: MarketplaceOffer) {
  return offer.sellerUserId === user.value?.id
}

function formatExpiresAt(offer: MarketplaceOffer) {
  return new Date(offer.expiresAt).toLocaleString()
}
</script>

<template>
  <UContainer
    class="
      py-8
      md:py-12
    "
  >
    <div class="mx-auto max-w-5xl space-y-6">
      <UiHeading title="Marketplace" />

      <UiCard>
        <template #header>
          <div class="flex items-center justify-between gap-3">
            <div class="text-lg font-semibold">
              Create Offer
            </div>
            <UBadge :color="canCreateOffer ? 'neutral' : 'error'" variant="subtle">
              {{ activeOffersCount }}/5 active
            </UBadge>
          </div>
        </template>

        <div
          class="
            grid grid-cols-1 gap-4
            md:grid-cols-2
          "
        >
          <div class="rounded-lg border border-default bg-elevated/30 p-4">
            <div class="mb-4 text-sm font-semibold tracking-wide text-muted uppercase">
              You give
            </div>

            <div class="space-y-4">
              <div class="space-y-2 rounded-md border border-default/60 p-3">
                <div class="text-xs font-semibold tracking-wide text-muted uppercase">
                  Gold
                </div>
                <UInput
                  v-model.number="offeredGoldAmount"
                  type="number"
                  min="0"
                  placeholder="Gold amount"
                />
              </div>

              <div class="space-y-2 rounded-md border border-default/60 p-3">
                <div class="text-xs font-semibold tracking-wide text-muted uppercase">
                  Loom
                </div>
                <UInput
                  v-model.number="offeredHeirloomAmount"
                  type="number"
                  min="0"
                  placeholder="Loom points"
                />
              </div>

              <div class="space-y-2 rounded-md border border-default/60 p-3">
                <div class="text-xs font-semibold tracking-wide text-muted uppercase">
                  Item
                </div>
                <USelect
                  v-model="offeredUserItemId"
                  :items="offeredItemOptions"
                  label-key="label"
                  value-key="value"
                  placeholder="Optional item"
                />
              </div>
            </div>
          </div>

          <div class="rounded-lg border border-default bg-elevated/30 p-4">
            <div class="mb-4 text-sm font-semibold tracking-wide text-muted uppercase">
              You want
            </div>

            <div class="space-y-4">
              <div class="space-y-2 rounded-md border border-default/60 p-3">
                <div class="text-xs font-semibold tracking-wide text-muted uppercase">
                  Gold
                </div>
                <UInput
                  v-model.number="requestedGoldAmount"
                  type="number"
                  min="0"
                  placeholder="Gold amount"
                />
              </div>

              <div class="space-y-2 rounded-md border border-default/60 p-3">
                <div class="text-xs font-semibold tracking-wide text-muted uppercase">
                  Loom
                </div>
                <UInput
                  v-model.number="requestedHeirloomAmount"
                  type="number"
                  min="0"
                  placeholder="Loom points"
                />
              </div>

              <div class="space-y-2 rounded-md border border-default/60 p-3">
                <div class="text-xs font-semibold tracking-wide text-muted uppercase">
                  Item
                </div>
                <USelect
                  v-model="requestedItemId"
                  :items="requestedItemOptions"
                  label-key="label"
                  value-key="value"
                  placeholder="Optional requested item"
                />
                <UInput
                  v-model.number="requestedItemAmount"
                  type="number"
                  min="1"
                  placeholder="Requested item count"
                  :disabled="!requestedItemId"
                />
              </div>
            </div>
          </div>
        </div>

        <div class="mt-4 rounded-lg border border-default bg-muted/30 p-4 text-sm">
          <div class="font-semibold">
            Preview
          </div>
          <div class="mt-2">
            <span class="font-semibold">You give:</span>
            {{ createOfferPreview.give.length ? createOfferPreview.give.join(', ') : 'none' }}
          </div>
          <div class="mt-1">
            <span class="font-semibold">You want:</span>
            {{ createOfferPreview.want.length ? createOfferPreview.want.join(', ') : 'none' }}
          </div>
        </div>

        <div class="mt-4 flex justify-end">
          <UButton
            :loading="creatingOffer"
            :disabled="isCreateDisabled || submitting"
            @click="onCreateOffer"
          >
            Publish offer
          </UButton>
        </div>
      </UiCard>

      <UiCard>
        <template #header>
          <div class="text-lg font-semibold">
            Active Offers
          </div>
        </template>

        <UiLoading :active="loadingOffers || loadingSelfOffers" />

        <div class="space-y-3">
          <div
            v-for="offer in offers"
            :key="offer.id"
            class="rounded-md border border-default p-3"
          >
            <div class="mb-2 flex items-start justify-between gap-3">
              <div>
                <div class="font-semibold">
                  #{{ offer.id }}
                </div>
                <div class="text-sm text-muted">
                  Expires: {{ formatExpiresAt(offer) }}
                </div>
              </div>
              <UBadge variant="subtle" color="neutral">
                {{ offer.status }}
              </UBadge>
            </div>

            <div
              class="
                grid grid-cols-1 gap-2 text-sm
                md:grid-cols-2
              "
            >
              <div>
                <span class="font-semibold">Give:</span>
                {{ summarizeAssets(offer, MARKETPLACE_ASSET_SIDE.Offered) }}
              </div>
              <div>
                <span class="font-semibold">Want:</span>
                {{ summarizeAssets(offer, MARKETPLACE_ASSET_SIDE.Requested) }}
              </div>
            </div>

            <div class="mt-3 flex justify-end gap-2">
              <UButton
                v-if="isSelfOffer(offer)"
                color="neutral"
                variant="outline"
                :loading="cancellingOffer"
                :disabled="submitting"
                @click="onCancelOffer(offer.id)"
              >
                Cancel
              </UButton>
              <UButton
                v-else
                :loading="acceptingOffer"
                :disabled="submitting"
                @click="onAcceptOffer(offer.id)"
              >
                Accept
              </UButton>
            </div>
          </div>

          <UiCard v-if="!offers.length && !loadingOffers" class="text-center text-sm text-muted">
            No active offers yet.
          </UiCard>
        </div>
      </UiCard>

      <UiCard>
        <template #header>
          <div class="text-lg font-semibold">
            My Offers
          </div>
        </template>

        <div class="space-y-2 text-sm">
          <div
            v-for="offer in selfOffers" :key="`self-${offer.id}`" class="
              rounded-md border border-default p-3
            "
          >
            <div class="flex items-center justify-between gap-3">
              <div>
                #{{ offer.id }}: {{ summarizeAssets(offer, MARKETPLACE_ASSET_SIDE.Offered) }} -> {{ summarizeAssets(offer, MARKETPLACE_ASSET_SIDE.Requested) }}
              </div>
              <UBadge variant="subtle" :color="offer.status === MARKETPLACE_OFFER_STATUS.Active ? 'success' : 'neutral'">
                {{ offer.status }}
              </UBadge>
            </div>
          </div>

          <UiCard v-if="!selfOffers.length" class="text-center text-sm text-muted">
            You have no offers yet.
          </UiCard>
        </div>
      </UiCard>
    </div>
  </UContainer>
</template> -->
