<script setup lang="ts">
import type { ItemFlat } from '~/models/item'

import { useItem } from '~/composables/item/use-item'
import { WeaponUsage } from '~/models/item'
import { weaponClassToIcon } from '~/services/item-service'

const { item, showTier = false } = defineProps<{
  item: ItemFlat
  showTier?: boolean
}>()

const { rankColor, thumb } = useItem(() => item)
</script>

<template>
  <div class="flex items-center gap-4">
    <div class="relative h-16 w-32">
      <VTooltip
        placement="auto"
        class="-mx-2"
        popper-class="expanded dense"
      >
        <img :src="thumb">
        <template #popper>
          <img :src="thumb">
          <div
            class="mt-2 p-4"
            :style="{ color: rankColor }"
          >
            {{ item.name }}
          </div>
        </template>
      </VTooltip>

      <!-- top-left -->
      <div class="absolute left-0 top-0 flex items-center gap-1.5">
        <ItemRankIcon
          v-if="item.rank > 0"
          class="cursor-default opacity-80 hover:opacity-100"
          :rank="item.rank"
        />

        <VTooltip
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
        </VTooltip>

        <slot name="top-left" />
      </div>

      <!-- bottom-right -->
      <div class="absolute bottom-0 right-0 flex items-center gap-1.5">
        <VTooltip
          v-if="showTier"
          placement="auto"
        >
          <Tag
            :label="String(item.tier)"
            size="sm"
          />

          <template #popper>
            <div class="prose prose-invert">
              <h5 class="text-content-100">
                {{ $t(`item.aggregations.tier.title`) }}
              </h5>
              <div v-html="$t(`item.aggregations.tier.description`)" />
            </div>
          </template>
        </VTooltip>

        <slot name="bottom-right" />
      </div>

      <!-- top-right -->
      <div class="absolute right-0 top-0 flex items-center gap-1.5">
        <Tag
          v-if="item.new === 1 && item.rank === 0"
          variant="success"
          label="new"
        />

        <slot name="top-right" />
      </div>
    </div>

    <div
      class="flex-1 text-2xs"
      :style="{ color: rankColor }"
    >
      {{ item.name }}
    </div>
  </div>
</template>
