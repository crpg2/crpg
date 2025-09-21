<script setup lang="ts">
import type { ItemFlat } from '~/models/item'

import { useItem } from '~/composables/item/use-item'
// import { WeaponUsage } from '~/models/item'
// import { weaponClassToIcon } from '~/services/item-service'

const { item, showTier = false } = defineProps<{
  item: ItemFlat
  showTier?: boolean
}>()

const { rankColor, thumb } = useItem(() => item)
</script>

<template>
  <div class="flex items-center gap-4">
    <div class="relative h-16 w-32 min-w-32">
      <UTooltip
        :content="{ side: 'right' }"
        :ui="{ content: 'p-5 h-auto w-[512px] flex-col gap-2' }"
      >
        <ItemThumb :thumb :name="item.name" />

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

        <!-- TODO: FIXME: -->
        <!-- <VTooltip
          v-if="item.weaponUsage.includes(WeaponUsage.Secondary)"
          placement="auto"
        >
          <Tag
            variant="warning"
            rounded
            icon="alert"
            size="sm"
          />

          <template #popper>
            <div class="prose prose-invert">
              <h5 class="text-status-warning">
                {{ $t(`shop.item.weaponUsage.title`) }}
              </h5>

              <p>
                {{ $t(`shop.item.weaponUsage.desc`) }}
              </p>

              <i18n-t
                scope="global"
                keypath="shop.item.weaponUsage.secondary"
                tag="p"
              >
                <template #weaponClass>
                  <div class="flex items-center gap-1 font-bold text-content-100">
                    <OIcon
                      size="lg"
                      :icon="weaponClassToIcon[item.weaponClass!]"
                    />
                    <span>{{ $t(`item.weaponClass.${item.weaponClass}`) }}</span>
                  </div>
                </template>
              </i18n-t>

              <i18n-t
                scope="global"
                keypath="shop.item.weaponUsage.primary"
                tag="p"
              >
                <template #weaponClass>
                  <div class="flex items-center gap-1 font-bold text-content-100">
                    <OIcon
                      size="lg"
                      :icon="weaponClassToIcon[item.weaponPrimaryClass!]"
                    />
                    <span>{{ $t(`item.weaponClass.${item.weaponPrimaryClass}`) }}</span>
                  </div>
                </template>
              </i18n-t>
            </div>
          </template>
        </VTooltip> -->

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
          v-if="item.new === 1 && item.rank === 0"
          color="success"
          label="new"
          size="sm"
          variant="subtle"
        />
        <slot name="top-right" />
      </div>
    </div>

    <span :style="{ color: rankColor }" class="whitespace-pre-wrap">
      {{ item.name }}
    </span>
  </div>
</template>
