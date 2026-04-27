<script setup lang="ts">
import type { SelectedItem } from '~/models/item'

import { getRankColor } from '~/services/item-service'

defineProps<{
  item: SelectedItem
  nextItem: SelectedItem
}>()

defineEmits<{
  close: [boolean]
}>()
</script>

<template>
  <AppConfirmActionDialog
    :title="$t('action.confirmation')"
    confirm="Upgrade item"
    @close="$emit('close', $event)"
  >
    <template #description>
      <i18n-t
        scope="global"
        keypath="character.inventory.item.upgrade.confirm.description"
        tag="div"
      >
        <template #loomPoints>
          <AppLoom :point="nextItem.rank - item.rank" />
        </template>
        <template #oldItem>
          <span :style="{ color: getRankColor(item.rank) }">
            {{ item.name }}
          </span>
        </template>
        <template v-if="nextItem" #newItem>
          <span :style="{ color: getRankColor(nextItem.rank) }">
            {{ nextItem.name }}
          </span>
        </template>
      </i18n-t>
    </template>
  </AppConfirmActionDialog>
</template>
