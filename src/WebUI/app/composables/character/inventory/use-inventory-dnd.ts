import { createSharedComposable, useToggle } from '@vueuse/core'
import { useI18n, useToast } from '#imports'
import { readonly, ref } from 'vue'

import type { EquippedItemId } from '~/models/character'
import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useCharacterItems } from '~/composables/character/use-character-items'
import { useUser } from '~/composables/user/use-user'
import { getAvailableSlotsByItem, getUnEquipItems } from '~/services/item-service'

export const _useInventoryDnD = () => {
  const { user } = useUser()

  const focusedItemId = ref<number | null>(null)
  const availableSlots = ref<ItemSlot[]>([])
  const fromSlot = ref<ItemSlot | null>(null)
  const toSlot = ref<ItemSlot | null>(null)

  const [dragging, toggleDragging] = useToggle()
  const toast = useToast()
  const { t } = useI18n()

  const { onUpdateCharacterItems, equippedItemsBySlot } = useCharacterItems()

  const onDragStart = (item: UserItem | null = null, slot: ItemSlot | null = null) => {
    toggleDragging(true)

    if (!item) {
      return
    }

    const { slots, warning } = getAvailableSlotsByItem(item, user.value!.id, equippedItemsBySlot.value)

    if (warning) {
      toast.add({
        title: t(warning),
        color: 'warning',
        close: false,
      })
      return
    }

    focusedItemId.value = item.id
    availableSlots.value = slots

    if (slot) {
      fromSlot.value = slot
    }
  }

  const onDragEnter = (slot: ItemSlot) => {
    toSlot.value = slot
  }

  const onDragLeave = () => {
    toSlot.value = null
  }

  const onDragEnd = (_e: DragEvent | null = null, slot: ItemSlot | null = null) => {
    if (slot && !toSlot.value) {
      onUpdateCharacterItems(getUnEquipItems(slot, equippedItemsBySlot.value))
    }

    focusedItemId.value = null
    availableSlots.value = []
    fromSlot.value = null
    toSlot.value = null
    toggleDragging(false)
  }

  const onDrop = (slot: ItemSlot) => {
    if (!availableSlots.value.includes(slot)) {
      return
    }

    const items: EquippedItemId[] = [{ slot, userItemId: focusedItemId.value }]

    // switch items (weapon)
    if (fromSlot.value) {
      items.push({
        slot: fromSlot.value,
        userItemId: equippedItemsBySlot.value[slot]?.id ?? null,
      })
    }

    onUpdateCharacterItems(items)
  }

  return {
    dragging: readonly(dragging),
    availableSlots: readonly(availableSlots),
    focusedItemId: readonly(focusedItemId),
    fromSlot: readonly(fromSlot),
    toSlot: readonly(toSlot),
    onDragEnd,
    onDragEnter,
    onDragLeave,
    onDragStart,
    onDrop,
  }
}

export const useInventoryDnD = createSharedComposable(_useInventoryDnD)
