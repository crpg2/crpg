<script setup lang="ts">
import { itemSellCostPenalty } from '~root/data/constants.json'

import type { UserItem } from '~/models/user'

import { computeSalePrice } from '~/services/item-service'

const {
  userItem,
} = defineProps<{
  userItem: UserItem
}>()

defineEmits<{
  sell: []
}>()

const userItemToReplaceSalePrice = computed(() => {
  const { graceTimeEnd, price } = computeSalePrice(userItem)
  return {
    graceTimeEnd: graceTimeEnd ? parseTimestamp(graceTimeEnd.valueOf() - new Date().valueOf()) : null,
    price,
  }
})

const isFreeRefund = computed(() => userItemToReplaceSalePrice.value.graceTimeEnd !== null)
</script>

<template>
  <UTooltip>
    <UButton
      variant="subtle"
      color="neutral"
      size="xl"
      block
      @click="$emit('sell')"
    >
      <AppCoin />
    </UButton>

    <template #content>
      <UiDataContent>
        <template #default="{ classes }">
          <i18n-t
            scope="global"
            keypath="character.inventory.item.sell.title"
            tag="div"
            :class="classes()"
          >
            <template #price>
              <AppCoin :value="userItemToReplaceSalePrice.price" />
            </template>
          </i18n-t>
        </template>

        <template #caption="{ classes }">
          <i18n-t
            v-if="isFreeRefund"
            scope="global"
            keypath="character.inventory.item.sell.freeRefund"
            tag="div"
            :class="classes()"
            class="text-success"
          >
            <template #dateTime>
              <span class="font-bold">
                {{ $t('dateTimeFormat.mm', { ...userItemToReplaceSalePrice.graceTimeEnd }) }}
              </span>
            </template>
          </i18n-t>

          <i18n-t
            v-else
            scope="global"
            keypath="character.inventory.item.sell.penaltyRefund"
            tag="div"
            :class="classes()"
            class="text-warning"
          >
            <template #penalty>
              <span class="font-bold">
                {{ $n(itemSellCostPenalty, 'percent', { minimumFractionDigits: 0 }) }}
              </span>
            </template>
          </i18n-t>
        </template>
      </UiDataContent>
    </template>
  </UTooltip>
</template>
