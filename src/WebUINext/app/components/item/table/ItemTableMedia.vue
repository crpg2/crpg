<script setup lang="ts">
import type { ItemFlat } from '~/models/item'

import { useItem } from '~/composables/item/use-item'
import { WEAPON_USAGE } from '~/models/item'
import { weaponClassToIcon } from '~/services/item-service'

const { item, showTier = false } = defineProps<{
  item: ItemFlat
  showTier?: boolean
}>()

const { rankColor, thumb } = useItem(() => item)
</script>

<template>
  <div class="flex items-center gap-2.5">
    <div class="relative h-16 w-32 min-w-32">
      <UTooltip
        :content="{ side: 'right' }"
        :ui="{ content: 'p-5 h-auto w-[512px] flex-col gap-2' }"
      >
        <div>
          <ItemThumb :thumb :name="item.name" />
        </div>

        <template #content>
          <div class="h-[240px] w-full">
            <ItemThumb :thumb :name="item.name" />
          </div>
          <div :style="{ color: rankColor }">
            {{ item.name }}
          </div>
        </template>
      </UTooltip>

      <!-- top-left -->
      <div class="absolute top-0 left-0 flex items-center gap-1.5">
        <ItemRankIcon
          v-if="item.rank > 0"
          class="
            cursor-default opacity-80
            hover:opacity-100
          "
          :rank="item.rank"
        />

        <UTooltip
          v-if="item.weaponUsage.includes(WEAPON_USAGE.Secondary)"
        >
          <UBadge
            color="warning"
            variant="subtle"
            icon="crpg:alert"
          />

          <template #content>
            <div class="prose prose-invert">
              <h3 class="text-warning">
                {{ $t('shop.item.weaponUsage.title') }}
              </h3>

              <p>{{ $t('shop.item.weaponUsage.desc') }}</p>

              <i18n-t
                scope="global"
                keypath="shop.item.weaponUsage.secondary"
                tag="p"
              >
                <template #weaponClass>
                  <UiDataMedia size="lg" :label="$t(`item.weaponClass.${item.weaponClass}`)" :icon="`crpg:${weaponClassToIcon[item.weaponClass!]}`" />
                </template>
              </i18n-t>

              <i18n-t
                scope="global"
                keypath="shop.item.weaponUsage.primary"
                tag="p"
              >
                <template #weaponClass>
                  <UiDataMedia size="lg" :label="$t(`item.weaponClass.${item.weaponPrimaryClass}`)" :icon="`crpg:${weaponClassToIcon[item.weaponPrimaryClass!]}`" />
                </template>
              </i18n-t>
            </div>
          </template>
        </UTooltip>

        <slot name="top-left" />
      </div>

      <!-- bottom-right -->
      <div class="absolute right-0 bottom-0 flex items-center gap-1.5">
        <UTooltip
          v-if="showTier"
          :content="{ side: 'right' }"
        >
          <UBadge
            :label="String(item.tier)"
            size="sm"
            variant="subtle"
          />

          <template #content>
            <UiTooltipContent :title="$t(`item.aggregations.tier.title`)" :description="$t(`item.aggregations.tier.description`)" />
          </template>
        </UTooltip>

        <slot name="bottom-right" />
      </div>

      <!-- top-right -->
      <div class="absolute top-0 right-0 flex items-center gap-1.5">
        <UBadge
          v-if="item.isNew && item.rank === 0"
          color="success"
          label="new"
          size="sm"
          variant="subtle"
        />
        <slot name="top-right" />
      </div>
    </div>

    <UiTextView variant="p-sm" :style="{ color: rankColor }" class="truncate whitespace-pre-wrap">
      {{ item.name }}
    </UiTextView>
  </div>
</template>
