import { createSharedComposable, tryOnScopeDispose } from '@vueuse/core'
import { ref } from 'vue'

interface ElementBound {
  x: number
  y: number
  width: number
}

interface OpenedItem {
  id: string
  userItemId: number
  bound: ElementBound
}

const getElementBounds = (el: HTMLElement) => {
  const { width, x, y } = el.getBoundingClientRect()
  return { width, x, y }
}

const computeDetailCardYPosition = (y: number, cardHeight = 700) => {
  // we cannot automatically determine the height of the card, so we take the maximum possible value
  // think about it, but it's fine as it is
  const bottomSpace = window.innerHeight - y
  return bottomSpace < cardHeight ? y + bottomSpace - cardHeight : y
}

export const _useItemDetail = () => {
  const openedItems = ref<OpenedItem[]>([])

  const isOpen = (id: string, userItemId: number) => openedItems.value.some(oi => oi.id === id && oi.userItemId === userItemId)

  const openItemDetail = (item: OpenedItem) => {
    openedItems.value.push(item)
  }

  const closeItemDetail = (item: OpenedItem) => {
    openedItems.value = openedItems.value.filter(oi => !(oi.id === item.id && oi.userItemId === item.userItemId))
  }

  const toggleItemDetail = (target: HTMLElement, item: Omit<OpenedItem, 'bound'>) => {
    const _item = { ...item, bound: getElementBounds(target) }

    isOpen(_item.id, _item.userItemId)
      ? closeItemDetail(_item)
      : openItemDetail(_item)
  }

  const closeAll = () => {
    openedItems.value = []
  }

  const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Escape' && openedItems.value.length > 0) {
      closeItemDetail(openedItems.value.at(-1)!)
    }
  }

  window.addEventListener('keydown', handleKeyDown)

  tryOnScopeDispose(() => {
    window.removeEventListener('keydown', handleKeyDown)
    closeAll()
  })

  return {
    openedItems,
    isOpen,
    closeItemDetail,
    toggleItemDetail,
    computeDetailCardYPosition,
  }
}

export const useItemDetail = createSharedComposable(_useItemDetail)
