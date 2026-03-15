import type { Ref } from 'vue'

import { beforeAll, beforeEach, describe, expect, it, vi } from 'vitest'
import { ref } from 'vue'

import type { MartetplaceSideFilter } from '~/models/marketplace'

import { useMarketplaceOffers } from '../use-marketplace-offers'

const {
  mockedUseRoute,
  mockedUseRouter,
  mockedGetMarketplaceOffers,
  mockedGetDefaultMartetplaceSideFilterState,
  mockedUseAsyncState,
  mockedUsePagination,
  mockedUseUser,
} = vi.hoisted(() => ({
  mockedUseRoute: vi.fn(),
  mockedUseRouter: vi.fn(),
  mockedGetMarketplaceOffers: vi.fn(),
  mockedGetDefaultMartetplaceSideFilterState: vi.fn(),
  mockedUseAsyncState: vi.fn(),
  mockedUsePagination: vi.fn(),
  mockedUseUser: vi.fn(),
}))

vi.mock('vue-router', () => ({
  useRoute: mockedUseRoute,
  useRouter: mockedUseRouter,
}))

vi.mock('@vueuse/core', () => ({
  useAsyncState: mockedUseAsyncState,
}))

vi.mock('~/composables/utils/use-pagination', () => ({
  usePagination: mockedUsePagination,
}))

vi.mock('~/composables/user/use-user', () => ({
  useUser: mockedUseUser,
}))

vi.mock('~/services/marketplace-service', () => ({
  getMarketplaceOffers: mockedGetMarketplaceOffers,
  getDefaultMartetplaceSideFilterState: mockedGetDefaultMartetplaceSideFilterState,
}))

function getDefaultSideFilter(overrides?: Partial<MartetplaceSideFilter>): MartetplaceSideFilter {
  return {
    itemType: null,
    itemRanks: [],
    item: null,
    gold: 'Any',
    heirloomPoints: 'Any',
    ...(overrides ?? {}),
  }
}

describe('useMarketplaceOffers', () => {
  let replaceMock: ReturnType<typeof vi.fn>
  let loadMarketplaceOffersMock: ReturnType<typeof vi.fn>
  let paginationRef: Ref<{ pageIndex: number, pageSize: number }>

  beforeAll(() => {
    vi.stubGlobal('window', { scrollTo: vi.fn() })
  })

  beforeEach(() => {
    replaceMock = vi.fn()
    loadMarketplaceOffersMock = vi.fn()
    paginationRef = ref({ pageIndex: 0, pageSize: 5 })

    mockedUseRoute.mockReturnValue({ query: {} })
    mockedUseRouter.mockReturnValue({ replace: replaceMock })

    mockedUsePagination.mockReturnValue({
      pagination: paginationRef,
      setPagination: (payload: Partial<{ pageIndex: number, pageSize: number }>) => {
        paginationRef.value = {
          pageIndex: payload.pageIndex ?? paginationRef.value.pageIndex,
          pageSize: payload.pageSize ?? paginationRef.value.pageSize,
        }
      },
    })

    mockedGetDefaultMartetplaceSideFilterState.mockImplementation(() => getDefaultSideFilter())
    mockedGetMarketplaceOffers.mockResolvedValue({ items: [], totalCount: 0 })

    mockedUseUser.mockReturnValue({ user: ref(null) })

    mockedUseAsyncState.mockReturnValue({
      state: ref({ items: [], totalCount: 0 }),
      executeImmediate: loadMarketplaceOffersMock,
      isLoading: ref(false),
    })
  })

  it('builds default filter model from query safely', () => {
    mockedUseRoute.mockReturnValue({
      query: {
        offered: 'invalid',
        requested: ['invalid'],
        seller: 'abc',
        onlyAffordable: 'true',
      },
    })

    const { filterModel } = useMarketplaceOffers()

    expect(filterModel.value).toEqual({
      offered: getDefaultSideFilter(),
      requested: getDefaultSideFilter(),
      seller: null,
      onlyAffordable: true,
    })
  })

  it('onSearch sends undefined side filters when no differences', () => {
    mockedUseRoute.mockReturnValue({ query: { foo: 'bar' } })

    const { onSearch } = useMarketplaceOffers()

    onSearch()

    expect(loadMarketplaceOffersMock).toHaveBeenCalledTimes(1)
    expect(replaceMock).toHaveBeenCalledWith({
      query: {
        foo: 'bar',
        offered: undefined,
        requested: undefined,
        seller: undefined,
        onlyAffordable: undefined,
      },
    })
  })

  it('onSearch sends only changed keys in side filters', () => {
    const { onPageChange, onSearch, updateFilterModel, filterModel, pagination } = useMarketplaceOffers()

    onPageChange(3)

    const customItem = { id: 'itm_1', name: 'Item 1' } as any
    updateFilterModel({
      seller: 42,
      onlyAffordable: true,
      offered: {
        ...filterModel.value.offered,
        itemType: 'OneHandedWeapon' as any,
      },
      requested: {
        ...filterModel.value.requested,
        itemRanks: [2],
        item: customItem,
      },
    })

    onSearch()

    expect(pagination.value.pageIndex).toBe(0)
    expect(loadMarketplaceOffersMock).toHaveBeenCalledTimes(2)
    expect(replaceMock).toHaveBeenCalledWith({
      query: {
        offered: {
          itemType: 'OneHandedWeapon',
        },
        requested: {
          itemRanks: [2],
          item: customItem,
        },
        seller: 42,
        onlyAffordable: true,
      },
    })
  })

  it('onReset clears filters, resets pagination, and clears query keys', () => {
    mockedUseRoute.mockReturnValue({ query: { keep: 'x', seller: '123' } })

    const { filterModel, onPageChange, onReset, updateFilterModel, pagination } = useMarketplaceOffers()

    onPageChange(2)
    updateFilterModel({
      seller: 55,
      onlyAffordable: true,
      offered: {
        ...filterModel.value.offered,
        itemRanks: [3],
      },
    })

    onReset()

    expect(pagination.value.pageIndex).toBe(0)
    expect(filterModel.value).toEqual({
      offered: getDefaultSideFilter(),
      requested: getDefaultSideFilter(),
      seller: null,
      onlyAffordable: false,
    })
    expect(loadMarketplaceOffersMock).toHaveBeenCalledTimes(2)
    expect(replaceMock).toHaveBeenCalledWith({
      query: {
        keep: 'x',
        seller: undefined,
        offered: undefined,
        requested: undefined,
        onlyAffordable: undefined,
      },
    })
  })
})
