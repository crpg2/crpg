<script setup lang="ts">
import type { ItemFlat } from '~/models/item'

import { getRankColor } from '~/services/item-service'

defineProps<{
  item: ItemFlat
  nextItem?: ItemFlat
}>()

defineEmits<{
  close: [boolean]
}>()
</script>

<template>
  <AppConfirmActionDialog
    :title="$t('action.confirmation')"
    confirm="Upgrade item"
    @close="(res) => $emit('close', res)"
  >
    <template #title>
      <i18n-t
        scope="global"
        keypath="character.inventory.item.upgrade.confirm.description"
      >
        <template #loomPoints>
          <AppLoom :point="1" />
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
