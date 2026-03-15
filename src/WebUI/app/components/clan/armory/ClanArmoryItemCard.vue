<script setup lang="ts">
import type { ClanArmoryItem } from '~/models/clan'

const { clanArmoryItem } = defineProps<{
  clanArmoryItem: ClanArmoryItem
}>()
</script>

<template>
  <ItemCard
    class="cursor-pointer"
    :item="clanArmoryItem.item"
  >
    <template #badges-top-right>
      <ClanArmoryItemRelationBadge
        :lender="clanArmoryItem.lender"
        :borrower="clanArmoryItem.borrower"
      />
    </template>
  </ItemCard>
</template>
