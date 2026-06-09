<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { marketplaceActiveListingLimit, marketplaceListingDurationDays } from '~root/data/constants.json'

import type { MarketplaceListing } from '~/models/marketplace'

import { LazyMarketplaceCreateListingDrawer, MarketplaceListingActions, MarketplaceListingAssetView, UBadge, UiDataCell, UserMedia, UTooltip } from '#components'
import { useMarketplaceListings } from '~/composables/marketplace/use-marketplace-listings'
import { useUser } from '~/composables/user/use-user'
import { useUserItemsProvider } from '~/composables/user/use-user-items'
import { SomeRole } from '~/models/role'
import { acceptMarketplaceListing, createMarketplaceListing, deleteMarketplaceListing } from '~/services/marketplace-service'
import { computeLeftMs, daysToMs, parseTimestamp } from '~/utils/date'

definePageMeta({
  roles: SomeRole,
})

const { t, d } = useI18n()
const { user, fetchUser } = useUser()
const { data: userItems, refresh: refreshUserItems } = useUserItemsProvider()

const {
  pagination,
  filterModel,
  updateFilterModel,
  onReset,
  onSearch,
  marketplaceListings,
  loadMarketplaceListings,
  loadingMarketplaceListings,
  onPageChange,
  onToggleSelfListings,
} = useMarketplaceListings()

const overlay = useOverlay()

const [onCreateListing] = useAsyncCallback(async () => {
  overlay
    .create(LazyMarketplaceCreateListingDrawer)
    .open({
      onClose: async (value, offer) => {
        if (!value || !offer) {
          return
        }

        await createMarketplaceListing({
          offer: {
            gold: offer.offered.gold,
            heirloomPoints: offer.offered.heirloomPoints,
            userItemId: offer.offered.userItem?.userItemId ?? null,
            itemId: null,
          },
          request: {
            gold: offer.requested.gold,
            heirloomPoints: offer.requested.heirloomPoints,
            itemId: offer.requested.item?.id ?? null,
            userItemId: null,
          },
        })

        await Promise.all([
          loadMarketplaceListings(),
          fetchUser(),
          refreshUserItems(),
        ])
      },
    })
})

const [onDeleteListing] = useAsyncCallback(async (offerId: number) => {
  await deleteMarketplaceListing(offerId)

  await Promise.all([
    loadMarketplaceListings(),
    fetchUser(),
    refreshUserItems(),
  ])
})

const [onAcceptListing] = useAsyncCallback(async (offerId: number) => {
  await acceptMarketplaceListing(offerId)

  await Promise.all([
    loadMarketplaceListings(),
    fetchUser(),
    refreshUserItems(),
  ])
})

const columns: TableColumn<MarketplaceListing>[] = [
  {
    accessorKey: 'seller',
    header: t('marketplace.page.columns.seller'),
    cell: ({ row }) => h(UserMedia, {
      user: row.original.seller,
      isSelf: user.value?.id === row.original.seller.id,
    }),
  },
  {
    accessorKey: 'offer',
    header: t('marketplace.page.columns.offer'),
    cell: ({ row }) => h(MarketplaceListingAssetView, { asset: row.original.offer }),
    meta: {
      class: {
        th: tw`w-[480px]`,
      },
    },
  },
  {
    header: t('marketplace.page.columns.request'),
    accessorKey: 'request',
    cell: ({ row }) => h(MarketplaceListingAssetView, { asset: row.original.request }),
    meta: {
      class: {
        th: tw`w-[480px]`,
      },
    },
  },
  {
    accessorKey: 'createdAt',
    header: t('marketplace.page.columns.expiresIn'),
    cell: ({ row }) => {
      const createdAt = new Date(row.original.createdAt)
      const expiresAt = new Date(createdAt.getTime() + daysToMs(marketplaceListingDurationDays))
      const leftMs = computeLeftMs(createdAt, daysToMs(marketplaceListingDurationDays))
      const remaining = parseTimestamp(leftMs)

      return h(UTooltip, {}, {
        default: () => h(UBadge, {
          label: t('dateTimeFormat.dd:hh:mm', { ...remaining }),
          icon: 'i-lucide-clock',
          variant: 'subtle',
        }),
        content: () => h('div', { class: 'space-y-2' }, [
          h(UiDataCell, null, {
            default: () => d(createdAt, 'short'),
            leftContent: () => t('marketplace.page.expiryTooltip.created'),
          }),
          h(UiDataCell, null, {
            default: () => d(expiresAt, 'short'),
            leftContent: () => t('marketplace.page.expiryTooltip.expires'),
          }),
        ]),
      })
    },
  },
  {
    id: 'actions',
    cell: ({ row }) => h(MarketplaceListingActions, {
      listing: row.original,
      user: user.value!,
      userItems: userItems.value,
      onDelete: () => onDeleteListing(row.original.id),
      onAccept: () => onAcceptListing(row.original.id),
    }),
  },
]

const hasReachedMarketplaceListingLimit = computed(() => user.value!.activeMarketplaceListingsCount >= marketplaceActiveListingLimit)
</script>

<template>
  <UContainer class="max-w-full space-y-8 py-12">
    <div class="grid grid-cols-[480px_1fr] gap-8">
      <div class="space-y-8">
        <UFieldGroup class="w-full" size="xl">
          <UButton
            :label="$t('marketplace.page.controls.searchMode')"
            block
            active-variant="solid"
            :active="filterModel.seller !== user?.id"
            icon="i-lucide-search"
            variant="subtle"
            @click="onToggleSelfListings"
          />

          <UButton
            :label="$t('marketplace.page.controls.myListings', { count: user!.activeMarketplaceListingsCount, limit: marketplaceActiveListingLimit })"
            variant="subtle"
            icon="crpg:member"
            block
            active-variant="solid"
            :active="filterModel.seller === user?.id"
            @click="onToggleSelfListings"
          />

          <UTooltip :disabled="!hasReachedMarketplaceListingLimit" :text="$t('marketplace.page.controls.offerLimitReached')">
            <UButton
              :label="$t('marketplace.page.controls.createListing')"
              block
              :disabled="hasReachedMarketplaceListingLimit"
              variant="subtle"
              icon="crpg:add"
              @click="onCreateListing"
            />
          </UTooltip>

          <UButton
            block
            variant="subtle"
            icon="i-lucide-history"
            :to="{ name: 'marketplace-history' }"
          />
        </UFieldGroup>

        <div class="space-y-8" :class="{ 'pointer-events-none opacity-50': filterModel.seller === user?.id }">
          <MarketplaceListingSideFilter
            :label="$t('marketplace.page.filters.offered')"
            :model-value="filterModel.offered"
            @update:model-value="(offered) => {
              updateFilterModel({ offered })
            }"
          />

          <MarketplaceListingSideFilter
            :label="$t('marketplace.page.filters.requested')"
            :model-value="filterModel.requested"
            @update:model-value="(requested) => {
              updateFilterModel({ requested })
            }"
          />

          <UCard
            variant="subtle"
            class="sticky bottom-4 z-10"
            :ui="{
              root: 'bg-elevated/80 backdrop-blur-md',
              body: '',
            }"
          >
            <template #header>
              <div class="flex gap-2">
                <UCheckbox
                  :label="$t('marketplace.page.controls.onlyAffordable')"
                  :model-value="filterModel.onlyAffordable"
                  variant="card"
                  @update:model-value="(onlyAffordable) => {
                    updateFilterModel({ onlyAffordable: Boolean(onlyAffordable) })
                  }"
                />
              </div>
            </template>

            <div class="flex gap-4">
              <UButton
                :label="$t('marketplace.page.controls.reset')"
                size="xl"
                variant="subtle"
                block
                :loading="loadingMarketplaceListings"
                color="neutral"
                @click="onReset"
              />
              <UButton
                :label="$t('marketplace.page.controls.search')"
                size="xl"
                :loading="loadingMarketplaceListings"
                block
                @click="onSearch"
              />
            </div>
          </UCard>
        </div>
      </div>

      <div class="space-y-4">
        <UTable
          class="relative rounded-md border border-muted"
          :data="marketplaceListings.items"
          :loading="loadingMarketplaceListings"
          :columns
          :pagination-options="{
            manualPagination: true,
          }"
        >
          <template #empty>
            <UiResultNotFound />
          </template>
        </UTable>

        <UiGridPagination
          :page="pagination.pageIndex + 1"
          :size="pagination.pageSize"
          :total="marketplaceListings.totalCount"
          @update:page="page => onPageChange(page - 1)"
        />
      </div>
    </div>
  </UContainer>
</template>
