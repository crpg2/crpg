<script setup lang="ts">
import type { ItemStack } from '~/models/strategus/party'
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
  submit: [offer: any] // TODO: need a name
}>()

interface ItemGroup {
  item: ItemStack['item']
  totalCount: number
  initialGroupACount: number
  initialGroupBCount: number
  groupACount: number
  groupBCount: number
}

function getInitialState() {
  const itemsMap = new Map<
    string, // itemId
    ItemGroup
  >()

  for (const stack of from) {
    itemsMap.set(stack.item.id, {
      item: stack.item,
      totalCount: stack.count,
      initialGroupACount: stack.count,
      initialGroupBCount: 0,
      groupACount: stack.count,
      groupBCount: 0,
    })
  }

  for (const stack of to) {
    const existing = itemsMap.get(stack.item.id)
    if (existing) {
      existing.totalCount += stack.count
      existing.groupBCount += stack.count
    }
    else {
      itemsMap.set(stack.item.id, {
        item: stack.item,
        totalCount: stack.count,
        groupACount: 0,
        groupBCount: stack.count,
        initialGroupACount: 0,
        initialGroupBCount: stack.count,
      })
    }
  }

  return itemsMap
}

const modelValue = ref(getInitialState())

const getMaxCount = (itemId: string): number => modelValue.value.get(itemId)!.totalCount

type Group = 'GroupA' | 'GroupB'

const getValue = (itemId: string, group: Group): number => {
  const itemGroup = modelValue.value.get(itemId)
  return group === 'GroupA' ? itemGroup!.groupACount : itemGroup!.groupBCount
}

const setValue = (itemId: string, group: Group, value: number) => {
  // console.log({ itemId, group, value })

  const itemGroup = modelValue.value.get(itemId)

  if (group === 'GroupA') {
    itemGroup!.groupACount = value
    itemGroup!.groupBCount = itemGroup!.totalCount - value
    return
  }

  itemGroup!.groupBCount = value
  itemGroup!.groupACount = itemGroup!.totalCount - value
}

const dynamicItems = computed(() => {
  const resultA: ItemStack[] = []
  const resultB: ItemStack[] = []

  for (const itemGroup of modelValue.value.values()) {
    if (itemGroup.groupACount > 0) {
      resultA.push({
        item: itemGroup.item,
        count: itemGroup.groupACount,
      })
    }
    if (itemGroup.groupBCount > 0) {
      resultB.push({
        item: itemGroup.item,
        count: itemGroup.groupBCount,
      })
    }
  }

  return { resultA, resultB }
})

interface PublicApi {
  submit: () => void
}

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
</script>

<template>
  <UCard
    :ui="{
      header: 'grid grid-cols-2 gap-4',
    }"
  >
    <ItemGridTest
      v-model:sorting="sortingModel"
      :items-a="dynamicItems.resultA"
      :items-b="dynamicItems.resultB"
      :sorting-config="sortingConfig"
      size="md"
    >
      <template #item="itemGroup">
        <div class="flex flex-col">
          <ItemCard
            class="cursor-pointer"
            :item="itemGroup.item"
            @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, itemGroup.item.id)"
          />
          <UInputNumber
            class="w-full"
            :min="0"
            :max="getMaxCount(itemGroup.item.id)"
            :model-value="getValue(itemGroup.item.id, itemGroup.group)"
            @update:model-value="(count) => {
              setValue(itemGroup.item.id, itemGroup.group, count as number)
            }"
          />
          <USlider
            class="px-2"
            :min="0"
            :max="getMaxCount(itemGroup.item.id)"
            :model-value="getValue(itemGroup.item.id, itemGroup.group)"
            @update:model-value="(count) => {
              setValue(itemGroup.item.id, itemGroup.group, count as number)
            }"
          />
        </div>
      </template>
    </ItemGridTest>

    <!-- <pre>
      {{ [...modelValue].map(([id, val]) => ({
        id,
          groupACount: val.groupACount,
          groupBCount: val.groupBCount,
          initialGroupACount: val.initialGroupACount,
          initialGroupBCount: val.initialGroupBCount,
          totalCount: val.totalCount,
      })) }}
    </pre> -->
  </UCard>
</template>
