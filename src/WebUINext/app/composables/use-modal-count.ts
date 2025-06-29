const counter = ref<number>(0) // shared state, needed for multiply open modals at the same time

export const useModalCounter = () => {
  const increase = (): void => {
    counter.value++
  }

  const decrease = (): void => {
    if (counter.value > 0) {
      counter.value--
    }
  }

  return { counter: readonly(counter), decrease, increase }
}
