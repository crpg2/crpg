<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

import type { ItemType, WeaponClass } from '~/models/item'

import { ITEM_TYPE } from '~/models/item'
import {
  hasWeaponClassesByItemType,
  itemTypeToIcon,
  weaponClassToIcon,
} from '~/services/item-service'

const {
  itemTypes,
  weaponClasses = [],
  orientation = 'horizontal',
  withAllCategories = false,
  size = 'xl',
} = defineProps<{
  itemTypes: ItemType[]
  weaponClasses?: WeaponClass[]
  orientation?: 'vertical' | 'horizontal'
  withAllCategories?: boolean
  size?: 'md' | 'xl'
}>()

const itemType = defineModel<ItemType>('itemType', { default: () => ITEM_TYPE.Undefined })

const weaponClass = defineModel<WeaponClass | null>('weaponClass', { default: () => ITEM_TYPE.OneHandedWeapon })

const itemTypeOptions = computed(() => {
  return [
    ...(withAllCategories ? [{ value: ITEM_TYPE.Undefined, icon: 'crpg:grid' }] : []),
    ...itemTypes.map<TabsItem>(type => ({
      icon: `crpg:${itemTypeToIcon[type]}`,
      value: type,
    })),
  ]
})

const weaponClassOptions = computed(() => {
  return weaponClasses.map<TabsItem>(weaponClass => ({
    icon: `crpg:${weaponClassToIcon[weaponClass]}`,
    value: weaponClass,
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
    <template #leading="{ item: _item }">
      <UTooltip
        v-if="_item.icon"
        :content="{ side: orientation === 'horizontal' ? 'bottom' : 'right' }"
        :text="$t(`item.type.${_item.value}`)"
      >
        <UIcon
          :name="_item.icon"
          class="cursor-pointer outline-0 select-none"
          :class="[size === 'xl' ? 'size-9' : 'size-6']"
        />
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
          <template #leading="{ item: _item }">
            <UTooltip
              v-if="_item.icon"
              :content="{ side: orientation === 'horizontal' ? 'bottom' : 'right' }"
              :text="$t(`item.weaponClass.${_item.value}`)"
            >
              <UIcon
                :name="_item.icon"
                class="size-9 cursor-pointer outline-0 select-none"
              />
            </UTooltip>
          </template>
        </UTabs>
      </div>
    </template>
  </UTabs>
</template>
