<script setup lang="ts">
import { LazyCharacterInventoryItemUpgradesModal } from '#components'
import { itemSellCostPenalty } from '~root/data/constants.json'

import type { CompareItemsResult } from '~/models/item'
import type { UserItem, UserPublic } from '~/models/user'

import {
  canAddedToClanArmory,
  canUpgrade,
  computeBrokenItemRepairCost,
  computeSalePrice,
} from '~/services/item-service'
import { useUserStore } from '~/stores/user'
import { parseTimestamp } from '~/utils/date'

const {
  compareResult,
  equipped = false,
  userItem,
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

const { clan, user } = toRefs(useUserStore())

const userItemToReplaceSalePrice = computed(() => {
  const { graceTimeEnd, price } = computeSalePrice(userItem)
  return {
    graceTimeEnd:
      graceTimeEnd === null ? null : parseTimestamp(graceTimeEnd.valueOf() - new Date().valueOf()),
    price,
  }
})

const repairCost = computed(() => computeBrokenItemRepairCost(userItem.item.price))

const isOwnArmoryItem = computed(() => userItem.isArmoryItem && userItem.userId === user.value!.id)
const isSellable = computed(
  () => userItem.item.rank <= 0 && !userItem.isArmoryItem && !userItem.isPersonal,
)
const isUpgradable = computed(() => canUpgrade(userItem.item.type) && !userItem.isArmoryItem)
const isCanAddedToClanArmory = computed(() => canAddedToClanArmory(userItem.item.type) && !userItem.isPersonal)

const overlay = useOverlay()

const itemUpgradesModal = overlay.create(LazyCharacterInventoryItemUpgradesModal, {
  props: {
    userItem,
    onReforge: () => emit('reforge'),
    onUpgrade: () => emit('upgrade'),
  },
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
          variant="soft"
          color="success"
        />
      </UTooltip>

      <UTooltip v-if="userItem.isBroken" :text="$t('character.inventory.item.broken.tooltip.title')">
        <UBadge
          icon="crpg:error"
          variant="soft"
          color="error"
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
            variant="soft"
          />
        </UTooltip>
      </template>
    </template>

    <template #actions>
      <div class="flex flex-wrap items-center gap-2" />

      <AppConfirmActionPopover
        v-if="isSellable"
        :confirm-label="$t('action.sell')"
        :title="$t('character.inventory.item.sell.confirm')"
        @confirm="emit('sell')"
      >
        <UButton
          variant="subtle"
        >
          <i18n-t
            scope="global"
            keypath="character.inventory.item.sell.title"
          >
            <template #price>
              <AppCoin :value="userItemToReplaceSalePrice.price" />
            </template>
          </i18n-t>

          <UTooltip>
            <UBadge
              v-if="userItemToReplaceSalePrice.graceTimeEnd !== null"
              size="sm"
              variant="solid"
              color="success"
              :label="$n(1, 'percent', { minimumFractionDigits: 0 })"
            />
            <UBadge
              v-else
              size="sm"
              color="error"
              variant="solid"
              :label="$n(itemSellCostPenalty, 'percent', { minimumFractionDigits: 0 })"
            />

            <template #content>
              <i18n-t
                v-if="userItemToReplaceSalePrice.graceTimeEnd !== null"
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
              <i18n-t
                v-else
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
        </UButton>
      </AppConfirmActionPopover>

      <AppConfirmActionPopover
        v-if="userItem.isBroken"
        @confirm="emit('repair')"
      >
        <UButton
          icon="crpg:repair"
          color="error"
          variant="subtle"
        >
          <i18n-t
            scope="global"
            keypath="character.inventory.item.repair.title"
          >
            <template #price>
              <AppCoin :value="repairCost" />
            </template>
          </i18n-t>
        </UButton>
      </AppConfirmActionPopover>

      <UButton
        v-if="isUpgradable"
        variant="subtle"
        :label="$t('character.inventory.item.upgrade.upgradesTitle')"
        icon="crpg:blacksmith"
        @click="itemUpgradesModal.open()"
      />

      <template v-if="clan && isCanAddedToClanArmory">
        <AppConfirmActionPopover
          v-if="!userItem.isArmoryItem"
          :confirm-label="$t('action.ok')"
          :title="$t('clan.armory.item.add.confirm.description')"
          @confirm="$emit('addToClanArmory')"
        >
          <UButton
            variant="subtle"
            icon="crpg:armory"
            :label="$t('clan.armory.item.add.title')"
          />
        </AppConfirmActionPopover>

        <template v-else>
          <AppConfirmActionPopover
            v-if="isOwnArmoryItem"
            :confirm-label="$t('action.ok')"
            :title="$t('clan.armory.item.remove.confirm.description')"
            @confirm="$emit('removeFromClanArmory')"
          >
            <UButton
              color="warning"
              variant="subtle"
              icon="crpg:armory"
              :label="$t('clan.armory.item.remove.title')"
            />
          </AppConfirmActionPopover>

          <UButton
            v-else
            variant="subtle"
            icon="crpg:armory"
            :label="$t('clan.armory.item.return.title')"
            @click="$emit('returnToClanArmory')"
          />
        </template>
      </template>
    </template>
  </ItemDetail>
</template>
