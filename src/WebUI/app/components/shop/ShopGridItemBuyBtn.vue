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
      content: 'p-0 ring-0 max-w-xs',
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
      <UiCard
        :ui="{
          header: 'prose',
          body: 'prose space-y-4',
          footer: 'flex justify-center items-center gap-2',
        }"
        :label="$t('shop.item.buy.tooltip.buy')"
      >
        <div class="space-y-2">
          <UiDataCell>
            <template #leftContent>
              {{ $t('item.aggregations.upkeep.title') }}:
            </template>
            <AppCoin :value="$t('item.format.upkeep', { upkeep: $n(upkeep) })" />
          </UiDataCell>

          <UiDataCell>
            <template #leftContent>
              {{ $t('item.aggregations.price.title') }}:
            </template>
            <AppCoin :value="price" />
          </UiDataCell>
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

        <template #footer>
          <UButton
            variant="soft"
            icon="crpg:close"
            :label="$t('action.cancel')"
            @click="() => {
              toggle(false)
            }"
          />

          <UButton
            icon="crpg:check"
            :label="$t('shop.item.buy.tooltip.buy')"
            @click="() => {
              $emit('buy')
              toggle(false)
            }"
          />
        </template>
      </UiCard>
    </template>
  </UPopover>
</template>
