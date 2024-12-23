import { useModalCounter } from './use-modal-count'

it('increase counter on call', () => {
  const { counter, decrease, increase } = useModalCounter()

  expect(counter.value).toBe(0)

  increase()

  expect(counter.value).toBe(1)

  decrease()
  decrease()

  expect(counter.value).toBe(-1)
})
