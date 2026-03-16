// @vitest-environment jsdom

import { describe, expect, it, vi } from 'vitest'

import { _useItemDetail } from '../use-item-detail'

const onBeforeRouteLeaveHook = vi.hoisted(() => ({
  handler: undefined as undefined | (() => void),
}))

vi.mock('vue-router', () => ({
  onBeforeRouteLeave: (handler: () => void) => {
    onBeforeRouteLeaveHook.handler = handler
  },
}))

const mockEl = { getBoundingClientRect: vi.fn(() => ({ width: 120, x: 50, y: 200 })) } as unknown as HTMLElement

describe('_useItemDetail', () => {
  it('opens an item', () => {
    const { openedItems, toggleItemDetail } = _useItemDetail()

    expect(openedItems.value).toHaveLength(0)
    toggleItemDetail(mockEl, '1')
    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]).toMatchObject({
      id: '1',
      bound: { x: 50, y: 200, width: 120 },
    })
  })

  it('toggles item off when already open', () => {
    const { openedItems, toggleItemDetail } = _useItemDetail()

    toggleItemDetail(mockEl, '1')
    expect(openedItems.value).toHaveLength(1)

    toggleItemDetail(mockEl, '1')
    expect(openedItems.value).toHaveLength(0)
  })

  it('supports opening same item id with different additionalId', () => {
    const { openedItems, toggleItemDetail, closeItemDetail } = _useItemDetail()

    toggleItemDetail(mockEl, '1', 'a')
    toggleItemDetail(mockEl, '1', 'b')

    expect(openedItems.value).toHaveLength(2)
    expect(openedItems.value.map(i => i.additionalId)).toEqual(['a', 'b'])

    closeItemDetail('1', 'a')

    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]).toMatchObject({ id: '1', additionalId: 'b' })
  })

  it('closes all same-id items when additionalId is not provided', () => {
    const { openedItems, toggleItemDetail, closeItemDetail } = _useItemDetail()

    toggleItemDetail(mockEl, '1', 'a')
    toggleItemDetail(mockEl, '1', 'b')
    toggleItemDetail(mockEl, '2', 'c')

    closeItemDetail('1')

    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]).toMatchObject({ id: '2', additionalId: 'c' })
  })

  it('closes a specific item', () => {
    const { openedItems, toggleItemDetail, closeItemDetail } = _useItemDetail()

    toggleItemDetail(mockEl, '1')
    toggleItemDetail(mockEl, '2')

    expect(openedItems.value).toHaveLength(2)

    closeItemDetail('1')
    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]?.id).toBe('2')
  })

  it('computes detail card Y position correctly', () => {
    const { computeDetailCardYPosition } = _useItemDetail()

    vi.spyOn(window, 'innerHeight', 'get').mockImplementationOnce(() => 1000)

    // Plenty of space
    expect(computeDetailCardYPosition(100)).toBe(100)

    // Not enough space — should shift up
    const shifted = computeDetailCardYPosition(800)
    expect(shifted).toBeLessThan(800)
  })

  it('returns correct element bounds', () => {
    const { toggleItemDetail, openedItems } = _useItemDetail()
    toggleItemDetail(mockEl, '1')

    const bound = openedItems.value[0]?.bound
    expect(bound).toEqual({ width: 120, x: 50, y: 200 })
  })

  it('accepts precomputed bounds as target', () => {
    const { toggleItemDetail, openedItems } = _useItemDetail()

    toggleItemDetail({ width: 300, x: 10, y: 20 }, '1')

    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]?.bound).toEqual({ width: 300, x: 10, y: 20 })
  })

  it('handles Escape key to close last item when setListeners = true', () => {
    const { openedItems, toggleItemDetail } = _useItemDetail()

    toggleItemDetail(mockEl, 'a')
    toggleItemDetail(mockEl, 'b')

    expect(openedItems.value).toHaveLength(2)

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))

    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]?.id).toBe('a')
  })

  it('handles Escape key with duplicate item ids by additionalId', () => {
    const { openedItems, toggleItemDetail } = _useItemDetail()

    toggleItemDetail(mockEl, 'same-id', 'a')
    toggleItemDetail(mockEl, 'same-id', 'b')

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))

    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]).toMatchObject({ id: 'same-id', additionalId: 'a' })
  })

  it('removes keydown listener and clears openedItems on route leave', () => {
    const removeSpy = vi.spyOn(window, 'removeEventListener')

    const composable = _useItemDetail()
    composable.toggleItemDetail(mockEl, 'a')
    composable.toggleItemDetail(mockEl, 'b')
    expect(composable.openedItems.value).toHaveLength(2)

    onBeforeRouteLeaveHook.handler?.()

    expect(removeSpy).toHaveBeenCalledWith('keydown', expect.any(Function))

    expect(composable.openedItems.value).toHaveLength(0)
  })
})
