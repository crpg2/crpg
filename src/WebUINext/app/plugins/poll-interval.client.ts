type SubscriptionFn = () => Promise<unknown> | unknown
// const INTERVAL = 1000 * 60 // 1 min
const INTERVAL = 5000

export default defineNuxtPlugin(() => {
  const subscriptions = ref(new Map<symbol | string, SubscriptionFn>())

  const keys = computed(() => [...subscriptions.value.keys()])
  const timer = setInterval(async () => {
    for (const [id, fn] of subscriptions.value) {
      Promise.resolve()
        .then(fn)
        .catch((err) => {
          // console.error(`[poll:${String(id.description ?? id)}]`, err)
        })
    }
  }, INTERVAL)

  if (import.meta.hot) {
    import.meta.hot.dispose(() => clearInterval(timer))
  }

  return {
    provide: {
      poll: {
        subscribe: (id: symbol, fn: SubscriptionFn) => {
          subscriptions.value.delete(id)
          subscriptions.value.set(id, fn)
          return () => subscriptions.value.delete(id)
        },

        unsubscribe: (id: symbol) => {
          subscriptions.value.delete(id)
        },

        keys,
      },
    },
  }
})
