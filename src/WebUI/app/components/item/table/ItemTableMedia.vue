<script setup lang="ts">
import type { ItemFlat } from '~/models/item'

import { WEAPON_USAGE } from '~/models/item'
import { weaponClassToIcon } from '~/services/item-service'

const { item, showTier = false } = defineProps<{
  item: ItemFlat
  showTier?: boolean
}>()
</script>

<template>
  <ItemMedia
    :base-id="item.baseId"
    :rank="item.rank"
    :name="item.name"
  >
    <template #top-left>
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
    </template>

    <template #top-right>
      <UBadge
        v-if="item.isNew && item.rank === 0"
        color="success"
        label="new"
        size="sm"
        variant="subtle"
      />
    </template>

    <template #bottom-right>
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
          <UiTooltipContent
            :title="$t(`item.aggregations.tier.title`)"
            :description="$t(`item.aggregations.tier.description`)"
          />
        </template>
      </UTooltip>
    </template>

    <template #name-caption>
      <slot name="name-caption" />
    </template>
  </ItemMedia>
</template>
