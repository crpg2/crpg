<script setup lang="ts">
import { vOnLongPress } from '@vueuse/components'

import type { CharacterCharacteristics, CharacterOverallItemsStats, EquippedItemId } from '~/models/character'
import type { ItemSlot } from '~/models/item'
import type { UserItemsBySlot } from '~/models/user'

import { useInventoryDnD } from '~/composables/character/inventory/use-inventory-dnd'
import { useInventoryQuickEquip } from '~/composables/character/inventory/use-inventory-quick-equip'
import { useItemDetail } from '~/composables/character/inventory/use-item-detail'
import {
  getCharacterSlotsSchema,
  getOverallArmorValueBySlot,
  validateItemNotMeetRequirement,
} from '~/services/character-service'

const props = defineProps<{
  equippedItems: UserItemsBySlot
  characterCharacteristics: CharacterCharacteristics
  itemsStatsOverall: CharacterOverallItemsStats
}>()

defineEmits<{
  change: [items: EquippedItemId[]] // used in useInventoryDnD
}>()

const slotsSchema = getCharacterSlotsSchema()
const { toggleItemDetail } = useItemDetail()

const { onQuickUnEquip } = useInventoryQuickEquip(() => props.equippedItems)

const onClickInventoryDollSlot = (e: PointerEvent, slot: ItemSlot) => {
  if (props.equippedItems[slot] === undefined) {
    return
  }

  if (e.ctrlKey) {
    onQuickUnEquip(slot)
  }
  else {
    toggleItemDetail(e.target as HTMLElement, {
      id: props.equippedItems[slot].item.id,
      userItemId: props.equippedItems[slot].id,
    })
  }
}

const {
  availableSlots,
  fromSlot,
  dragging,
  onDragEnd,
  onDragEnter,
  onDragLeave,
  onDragStart,
  onDrop,
  toSlot,
} = useInventoryDnD(() => props.equippedItems)
</script>

<template>
  <div class="relative grid grid-cols-3 gap-4">
    <div class="absolute inset-0 -z-10 flex items-end justify-center">
      <UiSpriteSymbol
        name="body-silhouette"
        viewBox="0 0 970 2200"
        class="
          w-52
          2xl:w-64
        "
      />
    </div>

    <div
      v-for="(slotGroup, idx) in slotsSchema"
      :key="idx"
      class="flex flex-col gap-3"
      :class="[{ 'z-20': idx === 0 }, { 'z-10 justify-end': idx === 1 }]"
    >
      <CharacterInventoryDollSlot
        v-for="slot in slotGroup"
        :key="`${slot.key}_${equippedItems[slot.key]?.id ?? 'empty'}`"
        v-on-long-press="[() => !dragging && onQuickUnEquip(slot.key), { delay: 500 }]"
        :item-slot="slot.key"
        :placeholder="slot.placeholderIcon"
        :user-item="equippedItems[slot.key]"
        :not-meet-requirement="slot.key in equippedItems && validateItemNotMeetRequirement(equippedItems[slot.key].item, characterCharacteristics)"
        :available="Boolean(availableSlots.length && availableSlots.includes(slot.key))"
        :focused="toSlot === slot.key && availableSlots.includes(slot.key)"
        :armor-overall="getOverallArmorValueBySlot(slot.key, itemsStatsOverall)"
        :invalid=" Boolean(availableSlots.length && toSlot === slot.key && !availableSlots.includes(slot.key))"
        :remove="fromSlot === slot.key && !toSlot"
        draggable="true"
        @dragstart="onDragStart(equippedItems[slot.key], slot.key)"
        @dragend="e => onDragEnd(e, slot.key)"
        @drop="onDrop(slot.key)"
        @dragover.prevent="onDragEnter(slot.key)"
        @dragleave.prevent="onDragLeave"
        @dragenter.prevent
        @click="e => onClickInventoryDollSlot(e, slot.key)"
      />
    </div>
  </div>
</template>
