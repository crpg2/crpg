import type { LocationQuery, LocationQueryValueRaw } from 'vue-router'

import { useAsyncState } from '@vueuse/core'
import { isEqual } from 'es-toolkit'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import type { MartetplaceListingsFilter, MartetplaceListingsSideFilter } from '~/models/marketplace'

import { useUser } from '~/composables/user/use-user'
import { usePagination } from '~/composables/utils/use-pagination'
import { getDefaultMartetplaceSideFilterState, getMarketplaceListings } from '~/services/marketplace-service'

function getEmptyFilterModel(): MartetplaceListingsFilter {
  return {
    offered: getDefaultMartetplaceSideFilterState(),
    requested: getDefaultMartetplaceSideFilterState(),
    seller: null,
    onlyAffordable: false,
  }
}

function getDefaultFilterModel(routeQuery: LocationQuery): MartetplaceListingsFilter {
  const { offered, requested, seller, onlyAffordable } = routeQuery

  return {
    offered: {
      ...getDefaultMartetplaceSideFilterState(),
      ...(offered && typeof offered === 'object' && !Array.isArray(offered) ? offered as unknown as MartetplaceListingsSideFilter : {}),
    },
    requested: {
      ...getDefaultMartetplaceSideFilterState(),
      ...(requested && typeof requested === 'object' && !Array.isArray(requested) ? requested as unknown as MartetplaceListingsSideFilter : {}),
    },
    seller: seller && !Number.isNaN(Number(seller)) ? Number(seller) : null,
    onlyAffordable: onlyAffordable === 'true',
  }
}

const buildSideFilterQueryDiff = (
  sideFilter: MartetplaceListingsSideFilter,
  emptySideFilter: MartetplaceListingsSideFilter,
): LocationQueryValueRaw => {
  const changedEntries = Object.entries(sideFilter).reduce<Array<[string, unknown]>>((acc, [key, value]) => {
    const baseValue = emptySideFilter[key as keyof MartetplaceListingsSideFilter]

    if (!isEqual(value, baseValue)) {
      acc.push([key, value])
    }

    return acc
  }, [])

  if (changedEntries.length === 0) {
    return undefined
  }

  return Object.fromEntries(changedEntries) as unknown as LocationQueryValueRaw
}

export const useMarketplaceListings = () => {
  const { pagination, setPagination } = usePagination()

  const router = useRouter()
  const route = useRoute()
  const { user } = useUser()

  const filterModel = ref<MartetplaceListingsFilter>(getDefaultFilterModel(route.query))

  function updateFilterModel(partial: Partial<MartetplaceListingsFilter>) {
    filterModel.value = {
      ...filterModel.value,
      ...partial,
    }
  }

  const {
    state: marketplaceListings,
    executeImmediate: loadMarketplaceListings,
    isLoading: loadingMarketplaceListings,
  } = useAsyncState(
    () => getMarketplaceListings(pagination.value, filterModel.value),
    { items: [], totalCount: 0 },
    { resetOnExecute: false },
  )

  function onReset() {
    updateFilterModel(getEmptyFilterModel())
    setPagination({ pageIndex: 0 })
    loadMarketplaceListings()

    router.replace({
      query: {
        ...route.query,
        offered: undefined,
        requested: undefined,
        seller: undefined,
        onlyAffordable: undefined,
      },
    })

    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function onSearch() {
    setPagination({ pageIndex: 0 })
    loadMarketplaceListings()

    const emptyFilter = getEmptyFilterModel()

    router.replace({
      query: {
        ...route.query,
        offered: buildSideFilterQueryDiff(filterModel.value.offered, emptyFilter.offered),
        requested: buildSideFilterQueryDiff(filterModel.value.requested, emptyFilter.requested),
        seller: filterModel.value.seller === null
          ? undefined
          : filterModel.value.seller as unknown as LocationQueryValueRaw,
        onlyAffordable: filterModel.value.onlyAffordable === emptyFilter.onlyAffordable
          ? undefined
          : filterModel.value.onlyAffordable as unknown as LocationQueryValueRaw,
      },
    })

    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function onPageChange(pageIndex: number) {
    setPagination({ pageIndex })
    loadMarketplaceListings()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function onToggleSelfListings() {
    const active = Boolean(filterModel.value.seller)
    setPagination({ pageIndex: 0 })

    updateFilterModel({
      ...getEmptyFilterModel(),
      seller: active ? null : user.value!.id,
    })

    router.replace({
      query: {
        seller: active ? undefined : user.value!.id,
      },
    })

    loadMarketplaceListings()

    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return {
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
  }
}
