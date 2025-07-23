<script setup lang="ts">
import type { ClanArmoryItem } from '~/models/clan'
import type { UserPublic } from '~/models/user'

const { clanArmoryItem } = defineProps<{
  clanArmoryItem: ClanArmoryItem
  lender: UserPublic
  borrower: UserPublic | null
}>()
</script>

<template>
  <ItemCard
    class="cursor-pointer"
    :item="clanArmoryItem.item"
  >
    <template #badges-top-right>
      <ClanArmoryItemRelationBadge
        :lender="lender"
        :borrower="borrower"
      />
    </template>
  </ItemCard>
</template>
