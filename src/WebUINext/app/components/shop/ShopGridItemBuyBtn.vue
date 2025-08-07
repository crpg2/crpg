<script setup lang="ts">
import { groupBy } from 'es-toolkit'

import type { UserItem } from '~/models/user'

import { getRankColor } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

const { inInventoryItems, notEnoughGold, price, upkeep } = defineProps<{
  price: number
  upkeep: number
  inInventoryItems: UserItem[]
  notEnoughGold: boolean
}>()

defineEmits<{
  buy: []
}>()

const { user } = toRefs(useUserStore())

const groupedByRankInventoryItems = computed(() => groupBy(inInventoryItems, ui => ui.item.rank))

const isExpensive = computed(() => user.value!.gold - price < upkeep)

const [open, toggle] = useToggle()
</script>

<template>
  <UPopover
    v-model:open="open"
    :content="{
      side: 'left',
    }"
    :ui="{
      content: 'p-0 ring-0 max-w-sm',
    }"
  >
    <UButton
      variant="outline"
      size="lg"
      :disabled="notEnoughGold"
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
      <UBadge
        v-if="isExpensive"
        size="sm"
        color="error"
        icon="crpg:alert"
        variant="soft"
      />
    </UButton>

    <template #content>
      <UCard
        :ui="{ footer: 'flex  items-center gap-2', header: 'prose' }"
      >
        <template #header>
          <h5>{{ $t('shop.item.buy.tooltip.buy') }}</h5>
        </template>

        <div class="prose space-y-4">
          <div class="flex items-center gap-2">
            {{ $t('item.aggregations.upkeep.title') }}:
            <AppCoin>{{ $t('item.format.upkeep', { upkeep: $n(upkeep) }) }}</AppCoin>
          </div>

          <i18n-t
            v-if="inInventoryItems.length"
            scope="global"
            keypath="shop.item.buy.tooltip.inInventory"
            tag="p"
            class="leading-relaxed"
          >
            <template #items>
              <div
                v-for="(items, group, idx) in groupedByRankInventoryItems" :key="group"
                class="inline"
              >
                <span
                  class="font-semibold"
                  :style="{ color: getRankColor(items[0]!.item.rank) }"
                >
                  {{ items[0]!.item.name }} ({{ items.length }})</span>
                <!-- eslint-disable-next-line vue/singleline-html-element-content-newline -->
                <template v-if="idx + 1 < Object.keys(groupedByRankInventoryItems).length">, </template>
              </div>
            </template>
          </i18n-t>

          <p
            v-if="notEnoughGold"
            class="text-error"
          >
            {{ $t('shop.item.buy.tooltip.notEnoughGold') }}
          </p>

          <p
            v-else-if="isExpensive"
            class="text-warning"
          >
            {{ $t('shop.item.expensive') }}
          </p>
        </div>

        <template #footer>
          <UButton
            variant="soft"
            size="sm"
            block
            icon="crpg:close"
            :label="$t('action.cancel')"
            @click="() => {
              toggle(false)
            }"
          />

          <UButton
            size="sm"
            block
            icon="crpg:check"
            :label="$t('shop.item.buy.tooltip.buy')"
            @click="() => {
              $emit('buy')
              toggle(false)
            }"
          />
        </template>
      </UCard>
    </template>
  </UPopover>
</template>
