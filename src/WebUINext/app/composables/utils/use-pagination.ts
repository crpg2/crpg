import type { PaginationState } from '@tanstack/vue-table'

export const usePagination = () => {
  function getInitialPaginationState(): PaginationState {
    return {
      pageIndex: 0,
      pageSize: 10,
    }
  }

  const pagination = ref<PaginationState>(getInitialPaginationState())

  return {
    pagination,
    getInitialPaginationState,
  }
}
