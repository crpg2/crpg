type SubscriptionFn = () => Promise<any> | any
const INTERVAL = 1000 * 60 // 1 min
// const INTERVAL = 5000

export default defineNuxtPlugin(() => {
  const subscriptions = new Map<symbol, SubscriptionFn>()

  const timer = setInterval(async () => {
    for (const [id, fn] of subscriptions) {
      Promise.resolve()
        .then(fn)
        .catch((err) => {
          console.error(`[poll:${String(id.description ?? id)}]`, err)
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
          subscriptions.delete(id)
          subscriptions.set(id, fn)

          return () => subscriptions.delete(id)
        },

        unsubscribe: (id: symbol) => {
          subscriptions.delete(id)
        },

        getAll: () => {
          return [...subscriptions.keys()]
        },
      },
    },
  }
})
