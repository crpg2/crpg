<script setup lang="ts">
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
      :items-a="TODO:"
      :items-b="TODO:"
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
          <USlider
            class="px-2"
            :min="0"
            :max="TODO:"
            :model-value="TODO:"
            @update:model-value="(count) => {
              TODO:
            }"
          />
        </div>
      </template>
    </ItemGridTest>
  </UCard>
</template>
