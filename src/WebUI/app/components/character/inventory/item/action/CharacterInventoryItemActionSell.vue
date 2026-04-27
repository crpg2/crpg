<script setup lang="ts">
import { itemSellCostPenalty } from '~root/data/constants.json'

import type { UserItem } from '~/models/user'

import { checkAvailabilitySellUserItem, computeSalePrice } from '~/services/item-service'

const { userItem } = defineProps<{
  userItem: UserItem
}>()

defineEmits<{
  sell: []
}>()

const { t } = useI18n()

const userItemToReplaceSalePrice = computed(() => {
  const { graceTimeEnd, price } = computeSalePrice(userItem)
  return {
    graceTimeEnd: graceTimeEnd ? parseTimestamp(graceTimeEnd.valueOf() - new Date().valueOf()) : null,
    price,
  }
})

const isFreeRefund = computed(() => userItemToReplaceSalePrice.value.graceTimeEnd !== null)

const availableSellUserItem = computed(() => checkAvailabilitySellUserItem(t, userItem))

const [open, toggle] = useToggle()
</script>

<template>
  <UPopover
    v-model:open="open"
    :content="{ side: 'left' }"
    :ui="{
      content: 'p-0 ring-0 max-w-xs',
    }"
  >
    <UTooltip :content="{ side: 'right' }">
      <UButton
        variant="subtle"
        color="neutral"
        block
        :disabled="!availableSellUserItem[0]"
      >
        <AppCoin />
      </UButton>

      <template #content>
        <UiTooltipContent
          :title="$t('character.inventory.item.sell.action')"
          :validation="availableSellUserItem[1]"
        />
      </template>
    </UTooltip>

    <template #content>
      <UCard
        :ui="{
          body: 'prose space-y-4',
          footer: 'flex justify-center items-center gap-2',
        }"
      >
        <UiTooltipContent>
          <template #title>
            <UiTextView variant="h4" tag="h4">
              <i18n-t
                tag="span"
                scope="global"
                keypath="character.inventory.item.sell.title"
              >
                <template #price>
                  <AppCoin :value="userItemToReplaceSalePrice.price" size="xl" />
                </template>
              </i18n-t>
            </UiTextView>
          </template>

          <template #description>
            <UiTextView variant="caption">
              <i18n-t
                v-if="isFreeRefund"
                scope="global"
                keypath="character.inventory.item.sell.freeRefund"
                tag="span"
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
                tag="span"
                class="text-warning"
              >
                <template #penalty>
                  <span class="font-bold">
                    {{ $n(itemSellCostPenalty, 'percent', { minimumFractionDigits: 0 }) }}
                  </span>
                </template>
              </i18n-t>
            </UiTextView>
          </template>
        </UiTooltipContent>

        <template #footer>
          <UButton
            variant="soft"
            block
            :label="$t('action.cancel')"
            @click="() => {
              toggle(false)
            }"
          />

          <UButton
            block
            :label="$t('action.sell')"
            @click="() => {
              $emit('sell')
              toggle(false)
            }"
          />
        </template>
      </UCard>
    </template>
  </UPopover>
</template>
