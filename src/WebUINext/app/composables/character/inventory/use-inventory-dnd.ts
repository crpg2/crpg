import { useToggle } from '@vueuse/core'

import type { EquippedItemId } from '~/models/character'
import type { ItemSlot } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useInventoryEquipment } from '~/composables/character/inventory/use-inventory-equipment'
import { useCharacterItems } from '~/composables/character/use-character-items'
import { getAvailableSlotsByItem } from '~/services/item-service'

// Shared state
const focusedItemId = ref<number | null>(null)
const availableSlots = ref<ItemSlot[]>([])
const fromSlot = ref<ItemSlot | null>(null)
const toSlot = ref<ItemSlot | null>(null)

export const useInventoryDnD = () => {
  const [dragging, toggleDragging] = useToggle()
  const toast = useToast()
  const { t } = useI18n()

  const { onUpdateCharacterItems, equippedItemsBySlot } = useCharacterItems()
  const { getUnEquipItemsLinked, isEquipItemAllowed } = useInventoryEquipment()

  const onDragStart = (item: UserItem | null = null, slot: ItemSlot | null = null) => {
    toggleDragging(true)

    if (!item || !isEquipItemAllowed(item)) {
      return
    }

    focusedItemId.value = item.id
    const { slots, warning } = getAvailableSlotsByItem(item.item, toValue(equippedItemsBySlot))

    if (warning) {
      toast.add({
        title: t(warning),
        color: 'warning',
        close: false,
      })
    }

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
      const items: EquippedItemId[] = getUnEquipItemsLinked(slot, toValue(equippedItemsBySlot))
      onUpdateCharacterItems(items)
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
        userItemId: toValue(equippedItemsBySlot)[slot]?.id ?? null,
      })
    }

    onUpdateCharacterItems(items)
  }

  return {
    dragging: readonly(dragging),
    availableSlots: readonly(availableSlots),
    focusedItemId: readonly(focusedItemId),
    fromSlot: readonly(fromSlot),
    onDragEnd,
    onDragEnter,
    onDragLeave,
    onDragStart,
    onDrop,
    toSlot: readonly(toSlot),
  }
}
