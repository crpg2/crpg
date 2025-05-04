import { Language } from '~/models/language'

export const useLanguages = () => {
  const route = useRoute()
  const router = useRouter()

  const languagesModel = computed({
    get() {
      return (route.query?.languages as Language[]) || []
    },

    set(languages: Language[]) {
      router.replace({
        query: {
          ...route.query,
          languages,
        },
      })
    },
  })

  const languages = Object.keys(Language) as Language[]

  const resetLanguagesModel = () => {
    languagesModel.value = []
  }

  return {
    languages,
    languagesModel,
    resetLanguagesModel,
  }
}
