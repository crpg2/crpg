<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { LocationQuery, LocationQueryValueRaw } from 'vue-router'

import { AppCoin, MarketplaceHistoryFilterByUser, MarketplaceListingAssetView, UBadge, UiGridColumnHeader, UserMedia } from '#components'

import type { MarketplaceListingHistory, MartetplaceListingsHistoryFilter } from '~/models/marketplace'

import { useUser } from '~/composables/user/use-user'
import { SomeRole } from '~/models/role'
import { getMarketplaceListingsHistory } from '~/services/marketplace-service'

definePageMeta({
  roles: SomeRole,
})

const { t, d } = useI18n()
const { user } = useUser()

const { pagination, setPagination } = usePagination()
const router = useRouter()
const route = useRoute()

function getDefaultFilterModel(routeQuery: LocationQuery): MartetplaceListingsHistoryFilter {
  const { seller, buyer } = routeQuery

  return {
    seller: seller && !Number.isNaN(Number(seller)) ? Number(seller) : null,
    buyer: buyer && !Number.isNaN(Number(buyer)) ? Number(buyer) : null,
  }
}

const filterModel = ref<MartetplaceListingsHistoryFilter>(getDefaultFilterModel(route.query))

function updateFilterModel(partial: Partial<MartetplaceListingsHistoryFilter>) {
  filterModel.value = {
    ...filterModel.value,
    ...partial,
  }
}

const {
  state: marketplaceListingsHistory,
  executeImmediate: loadMarketplaceListingsHistory,
  isLoading: loadingMarketplaceListingsHistory,
} = useAsyncState(() => getMarketplaceListingsHistory(pagination.value, filterModel.value), {
  items: [],
  totalCount: 0,
}, {
  resetOnExecute: false,
})

function onPageChange(pageIndex: number) {
  setPagination({ pageIndex })
  loadMarketplaceListingsHistory()
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

function onFilterChange() {
  setPagination({ pageIndex: 0 })
  router.replace({
    query: {
      ...route.query,
      seller: filterModel.value.seller === null
        ? undefined
        : filterModel.value.seller as unknown as LocationQueryValueRaw,
      buyer: filterModel.value.buyer === null
        ? undefined
        : filterModel.value.buyer as unknown as LocationQueryValueRaw,
    },
  })
  loadMarketplaceListingsHistory()
}

function createUserFilterHeader(field: 'seller' | 'buyer') {
  return () => h(UiGridColumnHeader, {
    label: t(`marketplace.page.columns.${field}`),
    withFilter: true,
    filtered: Boolean(filterModel.value[field]),
    onResetFilter: () => {
      updateFilterModel({ [field]: null })
      onFilterChange()
    },
  }, {
    'filter-content': ({ toggle }: { toggle: (state: boolean) => void }) => {
      return h(MarketplaceHistoryFilterByUser, {
        'modelValue': filterModel.value[field],
        'onUpdate:modelValue': (value) => {
          setTimeout(() => {
            toggle(false)
          }, 100)
          updateFilterModel({ [field]: value })
          onFilterChange()
        },
      })
    },
  })
}

const columns: TableColumn<MarketplaceListingHistory>[] = [
  {
    accessorKey: 'id',
    id: 'actions',
    meta: {
      class: {
        th: tw`w-[88px]`,
      },
    },
  },
  {
    accessorKey: 'seller',
    cell: ({ row }) => h(UserMedia, {
      user: row.original.seller,
      isSelf: user.value?.id === row.original.seller.id,
    }),
    header: createUserFilterHeader('seller'),
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
    accessorKey: 'buyer',
    cell: ({ row }) => h(UserMedia, {
      user: row.original.buyer,
      isSelf: user.value?.id === row.original.buyer.id,
    }),
    header: createUserFilterHeader('buyer'),
  },
  {
    accessorKey: 'request',
    header: t('marketplace.page.columns.request'),
    cell: ({ row }) => h(MarketplaceListingAssetView, { asset: row.original.request }),
    meta: {
      class: {
        th: tw`w-[480px]`,
      },
    },
  },
  {
    accessorKey: 'goldFee',
    header: t('marketplace.page.columns.goldFee'),
    cell: ({ row }) => h(AppCoin, {
      value: row.original.goldFee,
    }),
  },
  {
    accessorKey: 'acceptedAt',
    header: t('marketplace.page.columns.acceptedAt'),
    cell: ({ row }) => {
      return h(UBadge, {
        label: d(row.original.acceptedAt, 'short'),
        icon: 'i-lucide-clock',
        variant: 'subtle',
      })
    },
  },
]
</script>

<template>
  <UContainer class="max-w-full space-y-8 py-12">
    <AppPageHeaderGroup
      :title="$t('marketplace.history.title')"
      :back-to="{ name: 'marketplace' }"
    />
    <div class="space-y-4">
      <UTable
        class="relative rounded-md border border-muted"
        :data="marketplaceListingsHistory.items"
        :loading="loadingMarketplaceListingsHistory"
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
        :total="marketplaceListingsHistory.totalCount"
        @update:page="page => onPageChange(page - 1)"
      />
    </div>
  </UContainer>
</template>
