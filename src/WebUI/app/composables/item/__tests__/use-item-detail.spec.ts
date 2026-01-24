// @vitest-environment jsdom

import { describe, expect, it, vi } from 'vitest'
import { effectScope } from 'vue'

import { _useItemDetail } from '../use-item-detail'

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

  it('handles Escape key to close last item when setListeners = true', () => {
    const { openedItems, toggleItemDetail } = _useItemDetail()

    toggleItemDetail(mockEl, 'a')
    toggleItemDetail(mockEl, 'b')

    expect(openedItems.value).toHaveLength(2)

    window.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }))

    expect(openedItems.value).toHaveLength(1)
    expect(openedItems.value[0]?.id).toBe('a')
  })

  it('removes keydown listener and clears openedItems on scope dispose', () => {
    const removeSpy = vi.spyOn(window, 'removeEventListener')

    const scope = effectScope()
    let composable: ReturnType<typeof _useItemDetail>

    scope.run(() => {
      composable = _useItemDetail()
      composable.toggleItemDetail(mockEl, 'a')
      composable.toggleItemDetail(mockEl, 'b')
      expect(composable.openedItems.value).toHaveLength(2)
    })

    scope.stop()

    expect(removeSpy).toHaveBeenCalledWith('keydown', expect.any(Function))

    expect(composable!.openedItems.value).toHaveLength(0)
  })
})
