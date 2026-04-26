<script setup lang="ts">
import type { LocationQueryRaw } from 'vue-router'

import type { CompareItemsResult } from '~/models/item'
import type { UserItem, UserPublic } from '~/models/user'

import { AppCoin, UBadge, UButton, UTooltip } from '#components'
import { useUser } from '~/composables/user/use-user'
import { canUpgradeItem, computeBrokenItemRepairCost } from '~/services/item-service'

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

      <UTooltip v-if="userItem.isListedOnMarketplace" :text="$t('character.inventory.item.listedOnMarketplace.tooltip.title')">
        <UBadge
          variant="soft"
          color="primary"
          icon="crpg:trade"
          size="lg"
        />
      </UTooltip>
    </template>

    <template #actions>
      <div class="flex flex-col gap-2">
        <UFieldGroup orientation="vertical" size="xl">
          <CharacterInventoryItemActionSell
            :user-item
            @sell="$emit('sell')"
          />

          <UTooltip
            v-if="userItem.isBroken"
            :content="{ side: 'right' }"
          >
            <UButton
              variant="subtle"
              color="neutral"
              icon="crpg:repair"
              block
              @click="$emit('repair')"
            />
            <template #content>
              <UiTextView variant="h4" tag="h4">
                <i18n-t
                  scope="global"
                  keypath="character.inventory.item.repair.title"
                >
                  <template #price>
                    <AppCoin :value="repairCost" size="xl" />
                  </template>
                </i18n-t>
              </UiTextView>
            </template>
          </UTooltip>

          <UTooltip v-if="canUpgradeItem(userItem.item.type)" :content="{ side: 'right' }">
            <UButton
              variant="subtle"
              color="neutral"
              icon="crpg:blacksmith"
              block
              square
              @click="$emit('upgrades')"
            />

            <template #content>
              <UiTooltipContent :title="$t('character.inventory.item.upgrade.upgradesTitle')" />
            </template>
          </UTooltip>

          <CharacterInventoryItemActionClanArmory
            v-if="!!clan"
            :user-item
            @add="$emit('addToClanArmory')"
            @remove="$emit('removeFromClanArmory')"
            @return="$emit('returnToClanArmory')"
          />

          <UTooltip
            v-if="!userItem.isPersonal"
            :content="{ side: 'right' }"
          >
            <UButton
              variant="subtle"
              color="neutral"
              icon="crpg:trade"
              block
              square
              size="xl"
              :to="{
                name: 'marketplace',
                query: {
                  requested: {
                    item: {
                      id: userItem.item.id,
                      baseId: userItem.item.baseId,
                      rank: userItem.item.rank,
                      name: userItem.item.name,
                    },
                  },
                } as unknown as LocationQueryRaw,
              }"
            />

            <template #content>
              <UiTooltipContent :title="$t('character.inventory.item.showMarketplaceListings.title')" />
            </template>
          </UTooltip>
        </UFieldGroup>
      </div>
    </template>
  </ItemDetail>
</template>
