import type { PaginationState } from '@tanstack/vue-table'

export const usePagination = (initialState?: Partial<PaginationState>) => {
  function getInitialPaginationState(): PaginationState {
    return {
      pageIndex: 0,
      pageSize: 15,
      ...(initialState ?? {}),
    }
  }

  const pagination = ref<PaginationState>(getInitialPaginationState())

  function setPagination(payload: PaginationState) {
    pagination.value = payload
  }

  return {
    pagination,
    setPagination,
    getInitialPaginationState,
  }
}
