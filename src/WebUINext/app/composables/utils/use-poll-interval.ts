import type { WatchSource, WatchStopHandle } from 'vue'

interface PollOption {
  key: symbol
  fn: () => Promise<any> | any
  watch?: WatchSource
}

export const usePollInterval = (options: PollOption | PollOption[]) => {
  const { $poll } = useNuxtApp()
  const unsubscribes: (() => boolean)[] = []
  const stopWatches: WatchStopHandle[] = []

  const _options = Array.isArray(options) ? options : [options]

  // TODO: FIXME: исследовать, убрать onMounted
  onMounted(() => {
    for (const { key, fn, watch: watchSource } of _options) {
      if (watchSource) {
        stopWatches.push(watch(
          watchSource,
          () => {
            unsubscribes.push($poll.subscribe(key, fn))
          },
        ))
      }
      else {
        unsubscribes.push($poll.subscribe(key, fn))
      }
    }
  })

  // TODO: scope sispose
  onBeforeUnmount(() => {
    unsubscribes.forEach(unsubscribe => unsubscribe())
    stopWatches.forEach(stop => stop())
  })
}
