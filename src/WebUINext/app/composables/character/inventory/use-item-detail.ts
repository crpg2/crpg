import { useMagicKeys, whenever } from '@vueuse/core'

interface ElementBound {
  x: number
  y: number
  width: number
}

interface OpenedItem {
  id: string
  uniqueId?: string
  userItemId: number
  bound: ElementBound
}

// shared state
const openedItems = ref<OpenedItem[]>([])

const getUniqueId = (itemId: string, userItemId: number) => `${itemId}::${userItemId}`

const getElementBounds = (el: HTMLElement) => {
  const { width, x, y } = el.getBoundingClientRect()
  return { width, x, y }
}

export const useItemDetail = (setListeners: boolean = false) => {
  const openItemDetail = (item: OpenedItem) => {
    if (!openedItems.value.some(oi => oi.uniqueId === item.uniqueId)) {
      openedItems.value.push({ ...item, uniqueId: getUniqueId(item.id, item.userItemId) })
    }
  }

  const closeItemDetail = (item: OpenedItem) => {
    if (openedItems.value.some(oi => oi.uniqueId === item.uniqueId)) {
      openedItems.value = openedItems.value.filter(oi => oi.uniqueId !== item.uniqueId)
    }
  }

  const toggleItemDetail = (target: HTMLElement, item: Omit<OpenedItem, 'bound'>) => {
    const uniqueId = getUniqueId(item.id, item.userItemId)
    const _item = { ...item, bound: getElementBounds(target) }

    !openedItems.value.some(oi => oi.uniqueId === uniqueId)
      ? openItemDetail(_item)
      : closeItemDetail(_item)
  }

  const closeAll = () => {
    openedItems.value = []
  }

  const computeDetailCardYPosition = (y: number) => {
    // we cannot automatically determine the height of the card, so we take the maximum possible value
    // think about it, but it's fine as it is
    const cardHeight = 700

    const yDiff = window.innerHeight - y
    const needOffset = yDiff < cardHeight

    if (!needOffset) {
      return y
    }

    return y + yDiff - cardHeight
  }

  const { escape } = useMagicKeys()

  if (setListeners) {
    const unsubscribe = whenever(escape!, () => {
      openedItems.value.length !== 0 && closeItemDetail(openedItems.value[openedItems.value.length - 1]!)
    })

    tryOnScopeDispose(() => {
      unsubscribe()
      closeAll()
    })
  }

  return {
    closeAll,
    closeItemDetail,
    computeDetailCardYPosition,
    getElementBounds,
    getUniqueId,
    openedItems,
    openItemDetail,
    toggleItemDetail,
  }
}
