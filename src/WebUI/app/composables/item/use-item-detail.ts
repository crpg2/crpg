import { createSharedComposable } from '@vueuse/core'
import { ref } from 'vue'

interface ElementBound {
  x: number
  y: number
  width: number
}

interface OpenedItem {
  id: string // itemId
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

  const isOpen = (itemId: string) => openedItems.value.some(oi => oi.id === itemId)

  const openItemDetail = (item: OpenedItem) => {
    openedItems.value.push(item)
  }

  const closeItemDetail = (itemId: string) => {
    openedItems.value = openedItems.value.filter(oi => oi.id !== itemId)
  }

  const toggleItemDetail = (target: HTMLElement, itemId: string) => {
    if (isOpen(itemId)) {
      closeItemDetail(itemId)
      return
    }
    openItemDetail({ id: itemId, bound: getElementBounds(target) })
  }

  const closeAll = () => {
    openedItems.value = []
  }

  const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Escape' && openedItems.value.length > 0) {
      closeItemDetail(openedItems.value.at(-1)!.id)
    }
  }

  window.addEventListener('keydown', handleKeyDown)

  onBeforeRouteLeave(() => {
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
