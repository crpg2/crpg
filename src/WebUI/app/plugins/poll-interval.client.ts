type SubscriptionFn = () => Promise<unknown> | unknown

type Key = string

export default defineNuxtPlugin(() => {
  const { pollIntevalMs } = useAppConfig()
  const subscriptions = ref(new Map<Key, SubscriptionFn>())

  const keys = computed(() => [...subscriptions.value.keys()])

  const timer = setInterval(async () => {
    for (const [id, fn] of subscriptions.value) {
      Promise.resolve()
        .then(fn)
        .catch((err) => {
          console.error(`[poll:${String(id)}]`, err)
        })
    }
  }, pollIntevalMs)

  if (import.meta.hot) {
    import.meta.hot.dispose(() => clearInterval(timer))
  }

  return {
    provide: {
      poll: {
        subscribe: (id: Key, fn: SubscriptionFn) => {
          subscriptions.value.delete(id)
          subscriptions.value.set(id, fn)
          return () => subscriptions.value.delete(id)
        },

        unsubscribe: (id: Key) => {
          subscriptions.value.delete(id)
        },

        keys,
      },
    },
  }
})
