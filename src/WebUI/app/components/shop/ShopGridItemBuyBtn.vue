<script setup lang="ts">
import { groupBy } from 'es-toolkit'

import type { UserItem } from '~/models/user'

import { useUser } from '~/composables/user/use-user'

const { inInventoryItems, notEnoughGold, price, upkeep } = defineProps<{
  price: number
  upkeep: number
  inInventoryItems: UserItem[]
  notEnoughGold: boolean
}>()

defineEmits<{
  buy: []
}>()

const { user } = useUser()

const groupedByRankInventoryItems = computed(() => groupBy(inInventoryItems, ui => ui.item.rank))

const isExpensive = computed(() => user.value!.gold - price < upkeep)
</script>

<template>
  <AppConfirmActionPopover
    :content="{ side: 'left' }"
    :title="$t('shop.item.buy.tooltip.buy')"
    :confirm-label="$t('shop.item.buy.tooltip.buy')"
    :confirm-disabled="notEnoughGold"
    @confirm="$emit('buy')"
  >
    <UButton
      variant="outline"
      size="lg"
    >
      <AppCoin
        :value="price"
        :class="{ 'opacity-50': notEnoughGold }"
      />
      <UBadge
        v-if="inInventoryItems.length"
        size="sm"
        variant="soft"
        :label="inInventoryItems.length"
      />
    </UButton>

    <template #description-content>
      <div class="space-y-4">
        <div class="space-y-2">
          <UiDataCell>
            <template #leftContent>
              {{ $t('item.aggregations.upkeep.title') }}:
            </template>
            <AppCoin :value="$t('item.format.upkeep', { upkeep: $n(upkeep) })" />
          </UiDataCell>
          <UBadge v-if="isExpensive" variant="subtle" color="error" icon="crpg:alert">
            {{ $t('shop.item.expensive') }}
          </UBadge>
        </div>

        <div class="space-y-2">
          <UiDataCell>
            <template #leftContent>
              {{ $t('item.aggregations.price.title') }}:
            </template>
            <AppCoin :value="price" />
          </UiDataCell>
          <UBadge v-if="notEnoughGold" variant="subtle" color="error" icon="crpg:alert">
            {{ $t('shop.item.buy.tooltip.notEnoughGold') }}
          </UBadge>
        </div>

        <i18n-t
          v-if="inInventoryItems.length"
          scope="global"
          keypath="shop.item.buy.tooltip.inInventory"
          tag="div"
        >
          <template #items>
            <div class="mt-4 space-y-1.5">
              <div v-for="(items, group,) in groupedByRankInventoryItems" :key="group">
                <ItemMedia
                  :base-id="items[0]!.item.baseId"
                  :name="items[0]!.item.name"
                  :rank="items[0]!.item.rank"
                >
                  <template #top-right>
                    <UChip
                      :text=" items.length"
                      color="neutral"
                      :ui="{
                        base: 'h-3.5 min-w-3.5 text-[8px] bg-inverted/25 text-white ring-0',
                      }"
                    />
                  </template>
                </ItemMedia>
              </div>
            </div>
          </template>
        </i18n-t>
      </div>
    </template>
  </AppConfirmActionPopover>
</template>
