<!-- TODO: FIXME: need a name -->
<!-- TODO: FIXME: REFACTORING -->
<script setup lang="ts">
import { useThrottleFn } from '@vueuse/core'
import { ItemCard, ItemDetail, UBadge, UIcon, UiInputNumberSlider } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { ItemStack, ItemStackUpdate } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'

const { from, to } = defineProps<{
  from: ItemStack[]
  to: ItemStack[]
}>()

const emit = defineEmits<{
  close: [value: boolean]
  submit: [items: ItemStackUpdate[]]
}>()

const { n } = useI18n()
const itemsById = computed(() => {
  const record: Record<string, ItemStack['item']> = {}

  for (const stack of [...from, ...to]) {
    record[stack.item.id] = stack.item
  }

  return record
})

interface ItemGroup {
  initialGroupACount: number
  initialGroupBCount: number
  groupACount: number
  groupBCount: number
  totalCount: number
}

function getInitialState(): Record<string, ItemGroup> {
  const itemsMap: Record<string, ItemGroup> = {}

  for (const stack of from) {
    itemsMap[stack.item.id] = {
      initialGroupACount: stack.count,
      initialGroupBCount: 0,
      groupACount: stack.count,
      groupBCount: 0,
      totalCount: stack.count,
    }
  }

  for (const stack of to) {
    const existing = itemsMap[stack.item.id]
    if (existing) {
      existing.groupBCount += stack.count
      existing.initialGroupBCount += stack.count
      existing.totalCount += stack.count
    }
    else {
      itemsMap[stack.item.id] = {
        groupACount: 0,
        groupBCount: stack.count,
        initialGroupACount: 0,
        initialGroupBCount: stack.count,
        totalCount: stack.count,
      }
    }
  }

  return itemsMap
}

const modelValue = shallowRef<Record<string, ItemGroup>>(getInitialState())

const _setModelValue = (newValue: Record<string, ItemGroup>) => {
  modelValue.value = {
    ...modelValue.value,
    ...newValue,
  }
}

const setModelValue = useThrottleFn(_setModelValue, 30)

const getMaxCount = (itemId: string): number => modelValue.value[itemId]!.totalCount

type Group = 'GroupA' | 'GroupB'

const getInitialCount = (itemId: string, group: Group): number => {
  const itemGroup = modelValue.value[itemId]
  return group === 'GroupA' ? itemGroup!.initialGroupACount : itemGroup!.initialGroupBCount
}

const getCount = (itemId: string, group: Group): number => {
  const itemGroup = modelValue.value[itemId]
  return group === 'GroupA' ? itemGroup!.groupACount : itemGroup!.groupBCount
}

const setCount = (itemId: string, group: Group, value: number) => {
  const total = modelValue.value[itemId]!.totalCount
  const isGroupA = group === 'GroupA'

  setModelValue({
    [itemId]: {
      ...modelValue.value[itemId]!,
      groupACount: isGroupA ? value : total - value,
      groupBCount: isGroupA ? total - value : value,
    },
  })
}

const dynamicItems = computed(() => {
  const itemsA: ItemStack[] = []
  const itemsB: ItemStack[] = []

  for (const [itemId, itemGroup] of Object.entries(modelValue.value)) {
    const item = itemsById.value[itemId]!

    if (itemGroup.initialGroupACount || itemGroup.groupACount > 0) {
      itemsA.push({ item, count: itemGroup.groupACount })
    }
    if (itemGroup.initialGroupBCount || itemGroup.groupBCount > 0) {
      itemsB.push({ item, count: itemGroup.groupBCount })
    }
  }

  return { itemsA, itemsB }
})

interface PublicApi {
  submit: () => void
  reset: () => void
}

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}
const sortingModel = ref<string>('rank_desc')

const onTransferAllToGroup = (group: Group, itemIds: string[]) => {
  setModelValue(
    Object.entries(modelValue.value)
      .reduce((acc, [itemId, itemGroup]) => {
        if (itemIds.includes(itemId)) {
          acc[itemId] = {
            ...itemGroup,
            groupACount: group === 'GroupA' ? itemGroup.totalCount : 0,
            groupBCount: group === 'GroupB' ? itemGroup.totalCount : 0,
          }
        }
        return acc
      }, {} as Record<string, ItemGroup>),
  )
}

const onReset = () => {
  modelValue.value = getInitialState()
}

const onSubmit = () => {
  emit('submit', Object.entries(modelValue.value)
    // only changed items
    .filter(([, itemGroup]) => itemGroup.groupACount !== itemGroup.initialGroupACount)
    .map(([itemId, itemGroup]) => ({
      itemId,
      count: itemGroup.groupACount - itemGroup.initialGroupACount,
    })))
}

const { toggleItemDetail } = useItemDetail()

// TODO: can be moved to a separate SFC
const renderItem = (itemGroup: { item: ItemStack['item'], group: Group }) => {
  const initialCount = getInitialCount(itemGroup.item.id, itemGroup.group)
  const count = getCount(itemGroup.item.id, itemGroup.group)

  return h('div', [
    h(ItemCard, {
      class: 'cursor-pointer',
      item: itemGroup.item,
      onClick: (event: MouseEvent) => toggleItemDetail(event.currentTarget as HTMLElement, itemGroup.item.id),
    }, {
      'badges-bottom-right': () => h(UBadge, {
        variant: 'subtle',
        color: 'neutral',
      }, () => [
        n(initialCount),
        initialCount !== count && [
          h(UIcon, { name: 'i-lucide-chevron-right' }),
          n(count),
        ],
      ]),
    }),
    h(UiInputNumberSlider, {
      'min': 0,
      'max': getMaxCount(itemGroup.item.id),
      'modelValue': count,
      'onUpdate:modelValue': (count: number) => setCount(itemGroup.item.id, itemGroup.group, count),
    }),
  ])
}

const renderItemDetail = <T extends { id: string }>(opendeItem: T, compareItemsResult: GroupedCompareItemsResult[]) => {
  const item = itemsById.value[opendeItem.id]!

  if (!item) {
    return null
  }

  return h(ItemDetail, {
    item,
    compareResult: compareItemsResult.find(cr => cr.type === item.type)?.compareResult,
  })
}

defineExpose<PublicApi>({
  submit: onSubmit,
  reset: onReset,
})
</script>

<template>
  <UCard
    :ui="{
      footer: 'flex flex-row justify-end gap-4',
    }"
  >
    <ItemGridSplit
      v-model:sorting="sortingModel"
      :items-a="dynamicItems.itemsA"
      :items-b="dynamicItems.itemsB"
      :sorting-config="sortingConfig"
    >
      <template #item="itemGroup">
        <component :is="renderItem(itemGroup)" />
      </template>

      <template #item-detail="{ item, compareItemsResult }">
        <component :is="renderItemDetail(item, compareItemsResult)" />
      </template>

      <template #left-side-header="{ filteredItemIds }">
        <div class="mb-2 flex flex-wrap items-center justify-between gap-4">
          <slot name="left-side-header" />
          <UButton
            color="neutral"
            trailing-icon="i-lucide-chevrons-right"
            variant="link"
            @click="onTransferAllToGroup('GroupB', filteredItemIds)"
          />
        </div>
      </template>

      <template #right-side-header="{ filteredItemIds }">
        <div class="mb-2 flex flex-wrap items-center justify-between gap-4">
          <UButton
            color="neutral"
            icon="i-lucide-chevrons-left"
            variant="link"
            @click="onTransferAllToGroup('GroupA', filteredItemIds)"
          />
          <slot name="right-side-header" />
        </div>
      </template>
    </ItemGridSplit>
  </UCard>
</template>
