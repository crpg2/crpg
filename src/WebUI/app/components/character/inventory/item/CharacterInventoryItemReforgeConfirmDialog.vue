<script setup lang="ts">
import type { ItemFlat } from '~/models/item'

import { getRankColor } from '~/services/item-service'

defineProps<{
  item: ItemFlat
  newItem: ItemFlat
  reforgeCost: number
}>()

defineEmits<{
  close: [boolean]
}>()
</script>

<template>
  <AppConfirmActionDialog
    :title="$t('action.confirmation')"
    confirm="Reforge item"
    @close="(res) => $emit('close', res)"
  >
    <template #description>
      <i18n-t
        scope="global"
        keypath="character.inventory.item.reforge.confirm.description"
        tag="div"
      >
        <template #gold>
          <AppCoin :value="reforgeCost" />
        </template>
        <template #loomPoints>
          <AppLoom :point="item.rank" />
        </template>
        <template #oldItem>
          <span
            class="font-bold"
            :style="{ color: getRankColor(item.rank) }"
          >
            {{ item.name }}
          </span>
        </template>
        <template #newItem>
          <span
            class="font-bold"
            :style="{ color: getRankColor(newItem.rank) }"
          >
            {{ newItem.name }}
          </span>
        </template>
      </i18n-t>
    </template>
  </AppConfirmActionDialog>
</template>
