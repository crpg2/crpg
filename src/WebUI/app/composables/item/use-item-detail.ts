import { createSharedComposable } from '@vueuse/core'
import { ref } from 'vue'
import { onBeforeRouteLeave } from 'vue-router'

import type { Item } from '~/models/item'

interface ElementBound {
  x: number
  y: number
  width: number
}

type Id = Item['id']

type AdditionalId = number | string

export interface OpenedItem {
  id: Id
  // There may be several identical items in the clan armory or in the inventory, so we need an additional identifier to distinguish them
  additionalId?: AdditionalId
  bound: ElementBound
}

const getElementBounds = (el: HTMLElement): ElementBound => {
  const { width, x, y } = el.getBoundingClientRect()
  return { width, x, y }
}

const computeDetailCardYPosition = (y: number, cardHeight = 700) => {
  // we cannot automatically determine the height of the card, so we take the maximum possible value
  // think about it, but it's fine as it is
  const bottomSpace = window.innerHeight - y
  return bottomSpace < cardHeight ? y + bottomSpace - cardHeight : y
}

const isOpened = (item: OpenedItem, id: Id, additionalId?: AdditionalId) => {
  return item.id === id && (additionalId === undefined || item.additionalId === additionalId)
}

export const _useItemDetail = () => {
  const openedItems = ref<OpenedItem[]>([])

  const isOpen = (id: Id, additionalId?: AdditionalId) => openedItems.value.some(oi => isOpened(oi, id, additionalId))

  const openItemDetail = (item: OpenedItem) => {
    openedItems.value.push(item)
  }

  const closeItemDetail = (id: Id, additionalId?: AdditionalId) => {
    openedItems.value = openedItems.value.filter(oi => !isOpened(oi, id, additionalId))
  }

  const toggleItemDetail = (target: HTMLElement | ElementBound, id: Id, additionalId?: AdditionalId): void => {
    if (isOpen(id, additionalId)) {
      closeItemDetail(id, additionalId)
      return
    }
    const bound = 'getBoundingClientRect' in target ? getElementBounds(target) : target

    openItemDetail({ id, additionalId, bound })
  }

  const closeAll = () => {
    openedItems.value = []
  }

  const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Escape') {
      const lastOpenedItem = openedItems.value[openedItems.value.length - 1]
      if (lastOpenedItem) {
        closeItemDetail(lastOpenedItem.id, lastOpenedItem.additionalId)
      }
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
