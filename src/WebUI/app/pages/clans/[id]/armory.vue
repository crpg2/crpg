<script setup lang="ts">
import { useStorage } from '@vueuse/core'

import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/character/inventory/use-item-detail'
import { useClan } from '~/composables/clan/use-clan'
import { useClanArmory } from '~/composables/clan/use-clan-armory'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import {
  getClanArmoryItemBorrower,
  getClanArmoryItemLender,
  isOwnClanArmoryItem,
} from '~/services/clan-service'

definePageMeta({
  props: true,
  roles: SomeRole,
  middleware: [
    'clan-foreign-validate',
  ],
})

const { t } = useI18n()

const { user } = useUser()
const { clan } = useClan()
const { clanMembers, getClanMember } = useClanMembers()

const {
  borrowItem,
  clanArmory,
  loadClanArmory,
  removeItem,
  returnItem,
  getClanArmoryItem,
} = useClanArmory()

const [onBorrowFromClanArmory] = useAsyncCallback(async (userItemId: number) => {
  await borrowItem(userItemId)
  await loadClanArmory()
}, {
  successMessage: t('clan.armory.item.borrow.notify.success'),
  pageLoading: true,
})

const [onRemoveFromClanArmory] = useAsyncCallback(async (userItemId: number) => {
  await removeItem(userItemId)
  await loadClanArmory()
}, {
  successMessage: t('clan.armory.item.remove.notify.success'),
  pageLoading: true,
})

const [onReturnFromClanArmory] = useAsyncCallback(async (userItemId: number) => {
  await returnItem(userItemId)
  await loadClanArmory()
}, {
  successMessage: t('clan.armory.item.return.notify.success'),
  pageLoading: true,
})

const hideOwnedItemsModel = useStorage<boolean>('clan-armory-hide-owned-items', true)
const showOnlyAvailableItems = useStorage<boolean>('clan-armory-show-only-available-items', true)

const items = computed(() => {
  if (!clanArmory.value.length) {
    return []
  }

  return clanArmory.value.filter((armoryItem) => {
    // Hide owned items if enabled
    if (hideOwnedItemsModel.value && isOwnClanArmoryItem(armoryItem, user.value!.id)) {
      return false
    }

    // Show only available items if enabled
    if (showOnlyAvailableItems.value) {
      // Item is available if not borrowed, not owned, and not in user's inventory
      if (armoryItem.borrowerUserId || isOwnClanArmoryItem(armoryItem, user.value!.id)) {
        return false
      }
    }
    return true
  })
})

const sortingConfig: SortingConfig = {
  rank_desc: { field: 'rank', order: 'desc' },
  type_asc: { field: 'type', order: 'asc' },
}
const sortingModel = ref<string>('rank_desc')

const { closeItemDetail, toggleItemDetail } = useItemDetail()
</script>

<template>
  <UContainer class="space-y-12 py-6">
    <AppPageHeaderGroup
      :title="$t('clan.armory.title')"
      :back-to="{ name: 'clans-id', params: { id: clan.id } }"
    />

    <div class="mx-auto max-w-2xl">
      <ItemGrid
        v-if="Boolean(items.length)"
        v-model:sorting="sortingModel"
        :items="items"
        :sorting-config="sortingConfig"
      >
        <template #filter-leading>
          <UDropdownMenu
            size="xl"
            :items="[
              {
                label: t('clan.armory.filter.hideOwned'),
                type: 'checkbox',
                checked: hideOwnedItemsModel,
                onUpdateChecked(checked) {
                  hideOwnedItemsModel = checked
                },
              },
              {
                label: t('clan.armory.filter.showOnlyAvailable'),
                type: 'checkbox',
                checked: showOnlyAvailableItems,
                onUpdateChecked(checked) {
                  showOnlyAvailableItems = checked
                },
              },
            ]"
            :modal="false"
          >
            <UChip
              inset
              size="2xl"
              :show="hideOwnedItemsModel || showOnlyAvailableItems"
              :ui="{ base: 'bg-[var(--color-notification)]' }"
            >
              <UButton
                variant="subtle"
                color="neutral"
                size="xl"
                icon="crpg:dots"
              />
            </UChip>
          </UDropdownMenu>
        </template>

        <template #item="clanArmoryItem">
          <ClanArmoryItemCard
            :clan-armory-item="clanArmoryItem"
            :lender="getClanMember(clanArmoryItem.userId)!.user"
            :borrower="getClanArmoryItemBorrower(clanArmoryItem.borrowerUserId, clanMembers)"
            @click="(e: Event) => toggleItemDetail(e.target as HTMLElement, {
              id: clanArmoryItem.item.id,
              userItemId: clanArmoryItem.userItemId,
            })"
          />
        </template>

        <template #footer>
          <div />
        </template>
      </ItemGrid>
    </div>

    <ItemDetailGroup>
      <template #default="di">
        <ClanArmoryItemDetail
          :clan-armory-item="getClanArmoryItem(di.userItemId)!"
          :lender="getClanArmoryItemLender(getClanArmoryItem(di.userItemId)!.userId, clanMembers)!"
          :borrower="getClanArmoryItemBorrower(getClanArmoryItem(di.userItemId)!.borrowerUserId, clanMembers)"
          @borrow="() => {
            closeItemDetail(di);
            onBorrowFromClanArmory(di.userItemId);
          }"
          @remove="() => {
            closeItemDetail(di);
            onRemoveFromClanArmory(di.userItemId);
          }"
          @return="() => {
            closeItemDetail(di);
            onReturnFromClanArmory(di.userItemId);
          }"
        />
      </template>
    </ItemDetailGroup>
  </UContainer>
</template>
