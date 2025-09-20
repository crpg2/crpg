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
} = defineProps<{
  itemTypes: ItemType[]
  weaponClasses?: WeaponClass[]
  orientation?: 'vertical' | 'horizontal'
  withAllCategories?: boolean
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
    size="xl"
    :orientation
    :ui="{
      list: 'w-auto',
      root: 'flex-row',
      trigger: [
        'size-13 p-0',
        orientation === 'vertical' ? 'flex justify-center' : '',
      ],
    }"
  >
    <template #leading="{ item }">
      <UTooltip :text="$t(`item.type.${item.value}`)">
        <UIcon v-if="item.icon" :name="item.icon" class="size-[60%] outline-0 select-none" />
      </UTooltip>
    </template>

    <template
      v-if="hasWeaponClassesByItemType(itemType) && Boolean(weaponClassOptions.length)"
      #default="{ item }"
    >
      <div v-if="item.value === itemType && weaponClass" class="flex items-center">
        <UIcon name="crpg:chevron-right" />
        <UTabs
          v-model="weaponClass"
          :items="weaponClassOptions"
          :content="false"
          size="xl"
          variant="link"
          :orientation
          :ui="{
            list: 'w-auto',
            root: 'flex-row',
            leadingIcon: 'size-7',
            trigger: 'p-2',
            indicator: 'hidden',
          }"
        />
      </div>
    </template>
  </UTabs>
</template>
