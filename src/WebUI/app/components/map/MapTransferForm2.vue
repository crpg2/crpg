<script setup lang="ts">
import { useThrottleFn } from '@vueuse/core'
import { ItemDetail } from '#components'

import type { GroupedCompareItemsResult } from '~/models/item'
import type { ItemStack, ItemStackUpdate } from '~/models/strategus/party'
import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/item/use-item-detail'

const { from, to } = defineProps<{
  from: ItemStack[]
  to: ItemStack[]
}>()

const emit = defineEmits<{
  submit: [items: ItemStackUpdate[]]
}>()

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

const getValue = (itemId: string, group: Group): number => {
  const itemGroup = modelValue.value[itemId]
  return group === 'GroupA' ? itemGroup!.groupACount : itemGroup!.groupBCount
}

const setValue = (itemId: string, group: Group, value: number) => {
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
  const resultA: ItemStack[] = []
  const resultB: ItemStack[] = []

  for (const [itemId, itemGroup] of Object.entries(modelValue.value)) {
    const item = itemsById.value[itemId]!

    if (itemGroup.groupACount > 0) {
      resultA.push({ item, count: itemGroup.groupACount })
    }
    if (itemGroup.groupBCount > 0) {
      resultB.push({ item, count: itemGroup.groupBCount })
    }
  }

  return { resultA, resultB }
})

interface PublicApi {
  submit: () => void
}

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
  // TODO: FIXME: by count
}

const sortingModel = ref<string>('rank_desc')

const { toggleItemDetail } = useItemDetail()

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

const onTransferAllToGroup = (group: Group) => {
  setModelValue(
    Object.entries(modelValue.value)
      .reduce((acc, [itemId, itemGroup]) => {
        acc[itemId] = {
          ...itemGroup,
          groupACount: group === 'GroupA' ? itemGroup.totalCount : 0,
          groupBCount: group === 'GroupB' ? itemGroup.totalCount : 0,
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
    .filter(([_, itemGroup]) => itemGroup.groupACount !== itemGroup.initialGroupACount)
    .map(([itemId, itemGroup]) => ({
      itemId,
      count: itemGroup.groupACount - itemGroup.initialGroupACount,
    })))
}

defineExpose<PublicApi>({
  submit: onSubmit,
})
</script>

<template>
  <UCard
    :ui="{
      footer: 'flex flex-row justify-end gap-4',
    }"
  >
    <ItemGridTest
      v-model:sorting="sortingModel"
      :items-a="dynamicItems.resultA"
      :items-b="dynamicItems.resultB"
      :sorting-config="sortingConfig"
    >
      <template #item="itemGroup">
        <ItemStackInput
          :item="itemGroup.item"
          :max="getMaxCount(itemGroup.item.id)"
          :model-value="getValue(itemGroup.item.id, itemGroup.group)"
          @update:model-value="(count) => setValue(itemGroup.item.id, itemGroup.group, count)"
          @toggle-item-detail="toggleItemDetail"
        />
      </template>

      <template #item-detail="{ item, compareItemsResult }">
        <component :is="renderItemDetail(item, compareItemsResult)" />
      </template>

      <template #left-side-header>
        <div class="mb-2 flex justify-end">
          <UButton
            color="neutral"
            trailing-icon="i-lucide-chevrons-right"
            variant="link"
            size="sm"
            label="Transfer all"
            @click="onTransferAllToGroup('GroupB')"
          />
        </div>
      </template>
      <template #right-side-header>
        <div class="mb-2 flex">
          <UButton
            color="neutral"
            icon="i-lucide-chevrons-left"
            variant="link"
            size="sm"
            label="Transfer all"
            @click="onTransferAllToGroup('GroupA')"
          />
        </div>
      </template>
    </ItemGridTest>

    <template #footer>
      <UButton
        :label="$t('action.reset')"
        block
        color="neutral"
        variant="soft"
        @click="onReset"
      />
      <UButton
        :label="$t('action.submit')"
        block
        color="primary"
        variant="soft"
        @click="onSubmit"
      />
    </template>
  </UCard>
</template>
