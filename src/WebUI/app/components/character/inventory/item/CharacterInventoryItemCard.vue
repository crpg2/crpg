<script setup lang="ts">
import type { UserItem, UserPublic } from '~/models/user'

import { getItemGraceTimeEnd, isGraceTimeExpired } from '~/services/item-service'

const {
  equipped = false,
  notMeetRequirement = false,
  userItem,
} = defineProps<{
  userId: number
  userItem: UserItem
  equipped?: boolean
  notMeetRequirement?: boolean
  lender?: UserPublic | null
}>()

const isNew = computed(() => !isGraceTimeExpired(getItemGraceTimeEnd(userItem)))
</script>

<template>
  <ItemCard
    :item="userItem.item"
    :class="{ 'bg-gold/25!': userItem.isPersonal }"
  >
    <template #badges-top-right>
      <UTooltip v-if="userItem.isBroken" :text="$t('character.inventory.item.broken.tooltip.title')">
        <UBadge
          variant="soft"
          color="error"
          icon="crpg:error"
          class="cursor-default"
        />
      </UTooltip>

      <template v-if="userItem.isArmoryItem">
        <ClanArmoryItemRelationBadge
          v-if="lender && lender.id !== userId"
          :lender
          class="cursor-default"
        />

        <UTooltip v-else :text="$t('character.inventory.item.clanArmory.inArmory.title')">
          <UBadge
            color="primary"
            variant="soft"
            icon="crpg:armory"
            class="cursor-default"
          />
        </UTooltip>
      </template>
    </template>

    <template #badges-bottom-left>
      <UBadge
        v-if="isNew"
        color="success"
        variant="soft"
        label="new"
        size="sm"
      />
    </template>

    <template #badges-bottom-right>
      <UTooltip
        v-if="notMeetRequirement"
        :text="$t('character.inventory.item.requirement.tooltip.title')"
      >
        <UBadge
          variant="soft"
          color="error"
          icon="crpg:alert"
        />
      </UTooltip>

      <UTooltip
        v-if="equipped"
        :text="$t('character.inventory.item.equipped')"
      >
        <UBadge
          variant="soft"
          color="success"
          icon="crpg:check"
        />
      </UTooltip>
    </template>
  </ItemCard>
</template>
