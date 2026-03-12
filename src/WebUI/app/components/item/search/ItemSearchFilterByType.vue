<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

import type { ItemType, WeaponClass } from '~/models/item'
import type { FacetedItemTypes, FacetedWeaponClasses } from '~/services/item-search-service'

import { ITEM_TYPE } from '~/models/item'
import {
  hasWeaponClassesByItemType,
  itemTypeToIcon,
  weaponClassToIcon,
} from '~/services/item-service'

const {
  itemTypes,
  weaponClasses = [],
  totalCount,
  orientation = 'horizontal',
  withAllCategories = false,
  size = 'xl',
} = defineProps<{
  itemTypes: FacetedItemTypes[]
  totalCount?: number
  weaponClasses?: FacetedWeaponClasses[]
  orientation?: 'vertical' | 'horizontal'
  withAllCategories?: boolean
  size?: 'md' | 'xl'
}>()

const itemType = defineModel<ItemType>('itemType', { default: () => ITEM_TYPE.Undefined })

const weaponClass = defineModel<WeaponClass | null>('weaponClass', { default: () => ITEM_TYPE.OneHandedWeapon })

type ItemTypeTabItem = TabsItem & { value: ItemType, count?: number }

const itemTypeOptions = computed(() => {
  return [
    ...(withAllCategories
      ? [{ value: ITEM_TYPE.Undefined, icon: 'crpg:grid', count: totalCount ?? 0 } satisfies ItemTypeTabItem]
      : []),
    ...itemTypes.map<ItemTypeTabItem>(item => ({
      icon: `crpg:${itemTypeToIcon[item.value]}`,
      value: item.value,
      count: item.count,
    })),
  ]
})

const weaponClassOptions = computed(() => {
  return weaponClasses.map<TabsItem>(item => ({
    icon: `crpg:${weaponClassToIcon[item.value]}`,
    value: item.value,
    count: item.count,
  }))
})
</script>

<template>
  <UTabs
    v-model="itemType"
    :items="itemTypeOptions"
    :content="false"
    color="neutral"
    :size
    :orientation
    :ui="{
      list: 'w-auto',
      root: 'flex-row',
      trigger: [
        orientation === 'vertical' ? 'flex justify-center' : '',
        size === 'xl' ? 'p-2 min-w-13 h-13' : 'p-1 min-w-10 h-10',
      ],
    }"
  >
    <template #leading="{ item }">
      <UTooltip
        :content="{ side: orientation === 'horizontal' ? 'bottom' : 'right' }"
        :text="$t(`item.type.${item.value}`)"
      >
        <UChip
          :show="item.count !== undefined"
          :text="item.count"
          color="neutral"
          :ui="{
            base: 'h-3.5 min-w-3.5 text-[8px] bg-inverted/25 text-white ring-0',
          }"
        >
          <UIcon
            :name="item.icon"
            class="cursor-pointer outline-0 select-none"
            :class="[size === 'xl' ? 'size-9' : 'size-6']"
          />
        </UChip>
      </UTooltip>
    </template>

    <template
      v-if="hasWeaponClassesByItemType(itemType) && Boolean(weaponClassOptions.length)"
      #default="{ item }"
    >
      <div v-if="item.value === itemType && weaponClass" class="flex items-center">
        <UIcon name="crpg:chevron-right" class="size-4" />

        <UTabs
          v-model="weaponClass"
          :items="weaponClassOptions"
          :content="false"
          :size
          variant="link"
          color="neutral"
          :orientation
          :ui="{
            list: 'w-auto',
            root: 'flex-row',
            trigger: 'min-w-13 h-13 p-0',
            indicator: 'hidden',
          }"
        >
          <template #leading="{ item: weaponClassItem }">
            <UTooltip
              :content="{ side: orientation === 'horizontal' ? 'bottom' : 'right' }"
              :text="$t(`item.weaponClass.${weaponClassItem.value}`)"
            >
              <UChip
                :show="weaponClassItem.count !== undefined"
                :text="weaponClassItem.count"
                color="neutral"
                :ui="{
                  base: 'h-3.5 min-w-3.5 text-[8px] bg-inverted/25 text-white ring-0',
                }"
              >
                <UIcon
                  :name="weaponClassItem.icon"
                  class="size-9 cursor-pointer outline-0 select-none"
                />
              </UChip>
            </UTooltip>
          </template>
        </UTabs>
      </div>
    </template>
  </UTabs>
</template>
