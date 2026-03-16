<script setup lang="ts">
import { AppCoin, UBadge, UButton, UTooltip } from '#components'

import type { CompareItemsResult } from '~/models/item'
import type { UserItem, UserPublic } from '~/models/user'

import { useUser } from '~/composables/user/use-user'
import {
  canAddedToClanArmory,
  canSell,
  canUpgradeUserItem,
  computeBrokenItemRepairCost,
} from '~/services/item-service'

const {
  userItem,
  compareResult,
  equipped = false,
} = defineProps<{
  userItem: UserItem
  compareResult?: CompareItemsResult
  equipped?: boolean
  lender?: UserPublic | null
}>()

defineEmits<{
  sell: []
  repair: []
  upgrades: []
  addToClanArmory: []
  removeFromClanArmory: []
  returnToClanArmory: []
}>()

const { user, clan } = useUser()

const isSellable = computed(() => canSell(userItem))
const isUpgradable = computed(() => canUpgradeUserItem(userItem))
const isManageClanArmory = computed(() => !!clan.value && canAddedToClanArmory(userItem))
const repairCost = computed(() => computeBrokenItemRepairCost(userItem.item.price))
</script>

<template>
  <ItemDetail
    :item="userItem.item"
    :compare-result="compareResult"
    :class="{ 'bg-gold/25': userItem.isPersonal }"
  >
    <template #badges-bottom-left>
      <UTooltip v-if="equipped" :text="$t('character.inventory.item.equipped')">
        <UBadge
          icon="crpg:check"
          variant="subtle"
          color="success"
          size="lg"
        />
      </UTooltip>

      <UTooltip v-if="userItem.isBroken" :text="$t('character.inventory.item.broken.tooltip.title')">
        <UBadge
          icon="crpg:error"
          variant="subtle"
          color="error"
          size="lg"
        />
      </UTooltip>

      <template v-if="userItem.clanArmoryLender">
        <ClanArmoryItemRelationBadge
          v-if="userItem.clanArmoryLender.user.id !== user!.id"
          :lender="userItem.clanArmoryLender.user"
        />
        <UTooltip v-else :text="$t('character.inventory.item.clanArmory.inArmory.title')">
          <UBadge
            icon="crpg:armory"
            variant="subtle"
            size="lg"
          />
        </UTooltip>
      </template>
    </template>

    <template #actions>
      <div class="flex flex-col gap-2">
        <UFieldGroup orientation="vertical">
          <CharacterInventoryItemActionSell
            v-if="isSellable"
            :user-item
            @click="$emit('sell')"
          />

          <UTooltip v-if="userItem.isBroken">
            <UButton
              variant="subtle"
              color="neutral"
              icon="crpg:repair"
              block
              size="xl"
              @click="$emit('repair')"
            />
            <template #content>
              <i18n-t
                scope="global"
                keypath="character.inventory.item.repair.title"
              >
                <template #price>
                  <AppCoin :value="repairCost" />
                </template>
              </i18n-t>
            </template>
          </UTooltip>

          <UTooltip v-if="isUpgradable" :text="$t('character.inventory.item.upgrade.upgradesTitle')">
            <UButton
              variant="subtle"
              color="neutral"
              icon="crpg:blacksmith"
              block
              square
              size="xl"
              @click="$emit('upgrades')"
            />
          </UTooltip>

          <CharacterInventoryItemActionClanArmory
            v-if="isManageClanArmory"
            :user-item
            @add="$emit('addToClanArmory')"
            @remove="$emit('removeFromClanArmory')"
            @return="$emit('returnToClanArmory')"
          />
        </UFieldGroup>
      </div>
    </template>
  </ItemDetail>
</template>
