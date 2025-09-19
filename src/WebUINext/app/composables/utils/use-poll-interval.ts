// import type { WatchSource, WatchStopHandle } from 'vue'

interface PollOption {
  key: symbol
  fn: () => Promise<any> | any
  // watch?: WatchSource
}

export const usePollInterval = (options: PollOption | PollOption[]) => {
  const { $poll } = useNuxtApp()
  const unsubscribes: (() => boolean)[] = []
  // const stopWatches: WatchStopHandle[] = []
  const _options = Array.isArray(options) ? options : [options]

  // console.log('usePollInterval', options)

  tryOnMounted(() => {
    for (const {
      key,
      fn,
    // watch: watchSource
    } of _options) {
    // if (watchSource) {
    //   stopWatches.push(watch(
    //     watchSource,
    //     () => {
    //       unsubscribes.push($poll.subscribe(key, fn))
    //     },
    //   ))
    // }
    // else {
    //   unsubscribes.push($poll.subscribe(key, fn))
    // }
      console.log('subscribe', options)
      unsubscribes.push($poll.subscribe(key, fn))
    }
  })

  tryOnScopeDispose(() => {
    console.log('onScopeDispose', options)
    unsubscribes.forEach(unsubscribe => unsubscribe())
  })
}
