import type { WatchSource, WatchStopHandle } from 'vue'

interface PollOption {
  key: string
  fn: () => Promise<any> | any
  watch?: WatchSource
}

export const usePollInterval = (options: PollOption | PollOption[]) => {
  const { $poll } = useNuxtApp()
  const unsubscribes: (() => boolean)[] = []
  const stopWatches: WatchStopHandle[] = []
  const _options = Array.isArray(options) ? options : [options]

  for (const {
    key,
    fn,
    watch: watchSource,
  } of _options) {
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

  tryOnScopeDispose(() => {
    unsubscribes.forEach(unsubscribe => unsubscribe())
  })
}
