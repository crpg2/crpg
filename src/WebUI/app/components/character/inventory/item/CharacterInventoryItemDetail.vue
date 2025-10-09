<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import { LazyCharacterInventoryItemUpgradesModal } from '#components'
import { itemSellCostPenalty } from '~root/data/constants.json'

import type { CompareItemsResult } from '~/models/item'
import type { UserItem, UserPublic } from '~/models/user'

import { useUser } from '~/composables/user/use-user'
import {
  canAddedToClanArmory,
  canSell,
  canUpgradeUserItem,
  computeBrokenItemRepairCost,
  computeSalePrice,
} from '~/services/item-service'
import { parseTimestamp } from '~/utils/date'

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

const emit = defineEmits<{
  sell: []
  repair: []
  upgrade: []
  reforge: []
  addToClanArmory: []
  removeFromClanArmory: []
  returnToClanArmory: []
}>()

const { user, clan } = useUser()

const userItemToReplaceSalePrice = computed(() => {
  const { graceTimeEnd, price } = computeSalePrice(userItem)
  return {
    graceTimeEnd: graceTimeEnd ? parseTimestamp(graceTimeEnd.valueOf() - new Date().valueOf()) : null,
    price,
  }
})

const repairCost = computed(() => computeBrokenItemRepairCost(userItem.item.price))

const isOwnArmoryItem = computed(() => userItem.isArmoryItem && userItem.userId === user.value!.id)
const isSellable = computed(() => canSell(userItem))
const isUpgradable = computed(() => canUpgradeUserItem(userItem))
const isCanAddedToClanArmory = computed(() => canAddedToClanArmory(userItem))

const overlay = useOverlay()

const itemUpgradesModal = overlay.create(LazyCharacterInventoryItemUpgradesModal)

const { t } = useI18n()

const itemActions = computed(() => {
  const result: DropdownMenuItem[] = []

  if (isSellable.value) {
    result.push({
      slot: 'sell' as const,
      onSelect: () => {
        emit('sell')
      },
    })
  }

  if (userItem.isBroken) {
    result.push({
      slot: 'repair' as const,
      icon: 'crpg:repair',
      onSelect: () => {
        emit('repair')
      },
    })
  }

  if (isUpgradable.value) {
    result.push({
      slot: 'upgrades' as const,
      icon: 'crpg:blacksmith',
      label: t('character.inventory.item.upgrade.upgradesTitle'),
      onSelect: () => {
        itemUpgradesModal.open({
          userItem,
          gold: user.value!.gold,
          heirloomPoints: user.value!.heirloomPoints,
          onReforge: () => {
            itemUpgradesModal.close()
            emit('reforge')
          },
          onUpgrade: () => {
            itemUpgradesModal.close()
            emit('upgrade')
          },
        })
      },
    })
  }

  if (!!clan.value && isCanAddedToClanArmory.value) {
    if (!userItem.isArmoryItem) {
      result.push({
        slot: 'armory-add' as const,
        icon: 'crpg:armory',
        label: t('clan.armory.item.add.title'),
        onSelect: () => {
          emit('addToClanArmory')
        },
      })
    }
    else {
      if (isOwnArmoryItem.value) {
        result.push({
          slot: 'armory-remove' as const,
          icon: 'crpg:armory',
          label: t('clan.armory.item.remove.title'),
          onSelect: () => {
            emit('removeFromClanArmory')
          },
        })
      }
      else {
        result.push({
          slot: 'armory-return' as const,
          icon: 'crpg:armory',
          label: t('clan.armory.item.return.title'),
          onSelect: () => {
            emit('returnToClanArmory')
          },
        })
      }
    }
  }

  return result
})
</script>

<template>
  <ItemDetail
    :item="userItem.item"
    :compare-result="compareResult"
    :class="{ 'bg-gold/25': userItem.isPersonal }"
  >
    <template #badges-bottom-right>
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

      <template v-if="userItem.isArmoryItem">
        <ClanArmoryItemRelationBadge
          v-if="lender && lender.id !== user!.id"
          :lender
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
      <UDropdownMenu :items="itemActions" size="xl">
        <UButton variant="subtle" color="neutral" size="xl" icon="crpg:dots" />

        <template #sell-label>
          <div class="flex items-center gap-2">
            <i18n-t
              scope="global"
              keypath="character.inventory.item.sell.title"
            >
              <template #price>
                <AppCoin :value="userItemToReplaceSalePrice.price" />
              </template>
            </i18n-t>

            <UTooltip v-if="userItemToReplaceSalePrice.graceTimeEnd !== null">
              <UBadge
                color="success"
                variant="subtle"
                :label="$n(1, 'percent', { minimumFractionDigits: 0 })"
              />
              <template #content>
                <i18n-t
                  scope="global"
                  keypath="character.inventory.item.sell.freeRefund"
                  tag="div"
                >
                  <template #dateTime>
                    <span class="font-bold">
                      {{ $t('dateTimeFormat.mm', { ...userItemToReplaceSalePrice.graceTimeEnd }) }}
                    </span>
                  </template>
                </i18n-t>
              </template>
            </UTooltip>

            <UTooltip v-else>
              <UBadge
                color="error"
                variant="subtle"
                :label="$n(itemSellCostPenalty, 'percent', { minimumFractionDigits: 0 })"
              />
              <template #content>
                <i18n-t
                  scope="global"
                  keypath="character.inventory.item.sell.penaltyRefund"
                  tag="div"
                >
                  <template #penalty>
                    <span class="font-bold text-error">
                      {{ $n(itemSellCostPenalty, 'percent', { minimumFractionDigits: 0 }) }}
                    </span>
                  </template>
                </i18n-t>
              </template>
            </UTooltip>
          </div>
        </template>

        <template #repair-label>
          <i18n-t
            scope="global"
            keypath="character.inventory.item.repair.title"
          >
            <template #price>
              <AppCoin :value="repairCost" />
            </template>
          </i18n-t>
        </template>
      </UDropdownMenu>
    </template>
  </ItemDetail>
</template>
