<script setup lang="ts">
import type { ValueOf } from 'type-fest'

import { ItemDetail } from '#components'

import type { ItemStack, TransferOfferPartyUpdate } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'

const {
  from,
  to,
} = defineProps<{
  from: ItemStack[]
  to: ItemStack[]
}>()

const emit = defineEmits<{
  submit: [offer: TransferOfferPartyUpdate]
}>()

const ITEM_GROUP = {
  GroupA: 'GroupA',
  GroupB: 'GroupB',
} as const

type ItemGroup = ValueOf<typeof ITEM_GROUP>

function parseGroupedItemKey(key: string): { group: ItemGroup, itemId: string } {
  const [group, itemId] = key.split('|')
  return { group: group as ItemGroup, itemId: itemId! }
}

function stringifyGroupedItemKey(group: ItemGroup, itemId: string): string {
  return `${group}|${itemId}`
}

interface TransferOfferModel {
  items: Record<string, number> // itemGroup|itemId/count
}

interface PublicApi {
  submit: () => void
}

const toast = useToast()

const onSubmit = () => {

}

defineExpose<PublicApi>({
  submit: onSubmit,
})

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}

const sortingModel = ref<string>('rank_desc')

const { toggleItemDetail } = useItemDetail()

const offerModel = ref<TransferOfferModel>(
  {
    items: {},
  },
)

const computedFrom = computed<ItemStack[]>(() => {
  const result = new Map<string, ItemStack>(from.map(stack => [stack.item.id, { ...stack, count: stack.count }]))

  for (const [key, count] of Object.entries(offerModel.value.items)) {
    // if (!count) {
    //   continue
    // }

    const { group, itemId } = parseGroupedItemKey(key)

    if (group === ITEM_GROUP.GroupA) {
      const existing = result.get(itemId)
      if (existing) {
        existing.count -= count
        // if (existing.count <= 0) {
        //   result.delete(itemId)
        // }
      }
    }
    else if (group === ITEM_GROUP.GroupB) {
      const existing = result.get(itemId)
      if (existing) {
        existing.count += count
      }
      else {
        const fromTo = to.find(s => s.item.id === itemId)
        if (fromTo) {
          result.set(itemId, { ...fromTo, count })
        }
      }
    }
  }

  return Array.from(result.values())
  // .filter(s => s.count > 0)
})

const computedTo = computed<ItemStack[]>(() => {
  const result = new Map<string, ItemStack>(to.map(stack => [stack.item.id, { ...stack, count: stack.count }]))

  for (const [key, count] of Object.entries(offerModel.value.items)) {
    // if (!count) {
    //   continue
    // }

    const { group, itemId } = parseGroupedItemKey(key)

    if (group === ITEM_GROUP.GroupA) {
      const existing = result.get(itemId)
      if (existing) {
        existing.count += count
      }
      else {
        const fromFrom = from.find(s => s.item.id === itemId)
        if (fromFrom) {
          result.set(itemId, { ...fromFrom, count })
        }
      }
    }
    else if (group === ITEM_GROUP.GroupB) {
      const existing = result.get(itemId)
      if (existing) {
        existing.count -= count
        // if (existing.count <= 0) {
        //   result.delete(itemId)
        // }
      }
    }
  }

  return Array.from(result.values())
  // .filter(s => s.count > 0)
})

const getOriginalCount = (group: ItemGroup, itemId: string): number => {
  const sourceList = group === ITEM_GROUP.GroupA ? from : to
  const stack = sourceList.find(s => s.item.id === itemId)
  return stack?.count || 0
}

const getMaxCount = (group: ItemGroup, itemId: string): number => {
  const sourceList = group === ITEM_GROUP.GroupA ? from : to
  const stack = sourceList.find(s => s.item.id === itemId)

  const otherGroup = group === ITEM_GROUP.GroupA ? ITEM_GROUP.GroupB : ITEM_GROUP.GroupA
  const otherGroupCount = offerModel.value.items[stringifyGroupedItemKey(otherGroup, itemId)] || 0

  // if (otherGroupCount) {
  //   return otherGroupCount
  // }

  return stack?.count || otherGroupCount || 0
}

const getTargetCount = (group: ItemGroup, itemId: string): number => {
  const originalCount = getOriginalCount(group, itemId)
  const otherGroup = group === ITEM_GROUP.GroupA ? ITEM_GROUP.GroupB : ITEM_GROUP.GroupA

  const otherGroupCount = offerModel.value.items[stringifyGroupedItemKey(otherGroup, itemId)] || 0
  const currentGroupCount = offerModel.value.items[stringifyGroupedItemKey(group, itemId)] || 0

  return currentGroupCount ? originalCount - currentGroupCount : otherGroupCount

  // const currentGroupCount = offerModel.value.items[stringifyGroupedItemKey(group, itemId)] || 0
  // const sourceList = group === ITEM_GROUP.GroupA ? from : to
  // const stack = sourceList.find(s => s.item.id === itemId)
  // return stack?.count || 0
}

const getCurrentValue = (group: ItemGroup, itemId: string): number => {
  const otherGroup = group === ITEM_GROUP.GroupA ? ITEM_GROUP.GroupB : ITEM_GROUP.GroupA

  const otherGroupCount = offerModel.value.items[stringifyGroupedItemKey(otherGroup, itemId)] || 0
  const currentGroupCount = offerModel.value.items[stringifyGroupedItemKey(group, itemId)] || 0

  return currentGroupCount || otherGroupCount
}
</script>

<template>
  <UCard
    :ui="{
      header: 'grid grid-cols-2 gap-4',
    }"
  >
    <ItemGridTest
      v-model:sorting="sortingModel"
      :items-a="computedFrom"
      :items-b="computedTo"
      :sorting-config="sortingConfig"
      size="md"
    >
      <template #item="itemGroup">
        <div class="flex flex-col">
          <ItemCard
            class="cursor-pointer"
            :item="itemGroup.item"
            @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, itemGroup.item.id)"
          >
            <template #badges-bottom-right>
              <UBadge variant="subtle" color="neutral">
                {{ $n(getOriginalCount(itemGroup.group, itemGroup.item.id)) }}
                <UIcon name="i-lucide-chevron-right" />
                {{ $n(getTargetCount(itemGroup.group, itemGroup.item.id)) }}
                <!-- {{ $n(offerModel.items[stringifyGroupedItemKey(itemGroup.group, itemGroup.item.id)] || 0) }} -->
              </UBadge>
              <!-- <UBadge variant="subtle" color="neutral">
                {{ $n(getOriginalCount(itemGroup.group, itemGroup.item.id) - (offerModel.items[stringifyGroupedItemKey(itemGroup.group, itemGroup.item.id)] || 0)) }}
                <template v-if="offerModel.items[stringifyGroupedItemKey(itemGroup.group, itemGroup.item.id)]">
                  <UIcon name="i-lucide-chevron-right" />
                  {{ $n(offerModel.items[stringifyGroupedItemKey(itemGroup.group, itemGroup.item.id)] || 0) }}
                </template>
              </UBadge> -->
            </template>
          </ItemCard>

          <!-- {{ {
            max: getMaxCount(itemGroup.group, itemGroup.item.id),
            current: getCurrentValue(itemGroup.group, itemGroup.item.id),
            target: getTargetCount(itemGroup.group, itemGroup.item.id),
          } }} -->
          <USlider
            class="px-2"
            :min="0"
            :max="getMaxCount(itemGroup.group, itemGroup.item.id)"
            :model-value="getCurrentValue(itemGroup.group, itemGroup.item.id)"
            @update:model-value="(count) => {
              if (!count) {
                delete offerModel.items[stringifyGroupedItemKey(itemGroup.group, itemGroup.item.id)]
                return
              }
              offerModel.items[stringifyGroupedItemKey(itemGroup.group, itemGroup.item.id)] = count
            }"
          />
        </div>
      </template>
    </ItemGridTest>

    {{ offerModel }}
  </UCard>
</template>
