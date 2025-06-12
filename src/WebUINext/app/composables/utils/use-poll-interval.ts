type SubscriptionFn = () => Promise<any> | any

const INTERVAL = 1000 * 60 * 2 // 2 min

// global state
const subscriptions = new Map<symbol, SubscriptionFn>()

// TODO: to nuxt plugin
setInterval(() => {
  for (const fn of subscriptions.values()) {
    fn()
  }
}, INTERVAL)

export const usePollInterval = () => {
  const subscribe = (id: symbol, fn: SubscriptionFn) => {
    subscriptions.set(id, fn)
  }

  const unsubscribe = (id: symbol) => {
    subscriptions.delete(id)
  }

  return {
    subscribe,
    unsubscribe,
  }
}
