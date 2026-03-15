import type { LocationQuery, LocationQueryValueRaw } from 'vue-router'

import { useAsyncState } from '@vueuse/core'
import { isEqual } from 'es-toolkit'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import type { MartetplaceFilter, MartetplaceSideFilter } from '~/models/marketplace'

import { useUser } from '~/composables/user/use-user'
import { usePagination } from '~/composables/utils/use-pagination'
import { getDefaultMartetplaceSideFilterState, getMarketplaceOffers } from '~/services/marketplace-service'

function getEmptyFilterModel(): MartetplaceFilter {
  return {
    offered: getDefaultMartetplaceSideFilterState(),
    requested: getDefaultMartetplaceSideFilterState(),
    seller: null,
    onlyAffordable: false,
  }
}

function getDefaultFilterModel(routeQuery: LocationQuery): MartetplaceFilter {
  const { offered, requested, seller, onlyAffordable } = routeQuery

  return {
    offered: {
      ...getDefaultMartetplaceSideFilterState(),
      ...(offered && typeof offered === 'object' && !Array.isArray(offered) ? offered as unknown as MartetplaceSideFilter : {}),
    },
    requested: {
      ...getDefaultMartetplaceSideFilterState(),
      ...(requested && typeof requested === 'object' && !Array.isArray(requested) ? requested as unknown as MartetplaceSideFilter : {}),
    },
    seller: seller && !Number.isNaN(Number(seller)) ? Number(seller) : null,
    onlyAffordable: onlyAffordable === 'true',
  }
}

const buildSideFilterQueryDiff = (
  sideFilter: MartetplaceSideFilter,
  emptySideFilter: MartetplaceSideFilter,
): LocationQueryValueRaw => {
  const changedEntries = Object.entries(sideFilter).reduce<Array<[string, unknown]>>((acc, [key, value]) => {
    const baseValue = emptySideFilter[key as keyof MartetplaceSideFilter]

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

export const useMarketplaceOffers = () => {
  const { pagination, setPagination } = usePagination()

  const router = useRouter()
  const route = useRoute()
  const { user } = useUser()

  const filterModel = ref<MartetplaceFilter>(getDefaultFilterModel(route.query))

  function updateFilterModel(partial: Partial<MartetplaceFilter>) {
    filterModel.value = {
      ...filterModel.value,
      ...partial,
    }
  }

  const {
    state: marketplaceOffers,
    executeImmediate: loadMarketplaceOffers,
    isLoading: loadingMarketplaceOffers,
  } = useAsyncState(
    () => getMarketplaceOffers(pagination.value, filterModel.value),
    { items: [], totalCount: 0 },
    { resetOnExecute: false },
  )

  function onReset() {
    updateFilterModel(getEmptyFilterModel())
    setPagination({ pageIndex: 0 })
    loadMarketplaceOffers()

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
    loadMarketplaceOffers()

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
    loadMarketplaceOffers()
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  function onToggleSelfOffers() {
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

    loadMarketplaceOffers()

    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  return {
    pagination,
    filterModel,
    updateFilterModel,
    onReset,
    onSearch,
    marketplaceOffers,
    loadMarketplaceOffers,
    loadingMarketplaceOffers,
    onPageChange,
    onToggleSelfOffers,
  }
}
