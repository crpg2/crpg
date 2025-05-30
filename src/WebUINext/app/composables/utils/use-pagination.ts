const DEFAULT_PAGE = 1
const DEFAULT_PER_PAGE = 10

export const usePagination = () => {
  const route = useRoute()
  const router = useRouter()

  const pageModel = computed({
    get() {
      return route.query?.page ? Number(route.query.page) : DEFAULT_PAGE
    },

    set(val: number) {
      router.push({
        query: {
          ...route.query,
          page: val === DEFAULT_PAGE ? undefined : val,
        },
      })
    },
  })

  const perPageModel = computed({
    get() {
      return route.query?.perPage ? Number(route.query.perPage) : DEFAULT_PER_PAGE
    },

    set(val: number) {
      router.push({
        query: {
          ...route.query,
          perPage: val === DEFAULT_PER_PAGE ? undefined : val,
        },
      })
    },
  })

  const perPageConfig = [10, 15, 20] // TODO: to model?

  return {
    pageModel,
    perPage: DEFAULT_PER_PAGE,
    perPageConfig,
    perPageModel,
  }
}
