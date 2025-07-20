<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import { useStorage } from '@vueuse/core'

import type { SortingConfig } from '~/services/item-search-service'

import { useItemDetail } from '~/composables/character/inventory/use-item-detail'
import { useClan } from '~/composables/clan/use-clan'
import { useClanArmory } from '~/composables/clan/use-clan-armory'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import {
  getClanArmoryItemBorrower,
  getClanArmoryItemLender,
  isClanArmoryItemInInventory,
  isOwnClanArmoryItem,
} from '~/services/clan-service'

const props = defineProps<{
  id: string
}>()

definePageMeta({
  props: true,
  roles: SomeRole,
  middleware: [
    'clan-id-param-validate',
    'clan-foreign-validate',
  ],
})

const clanId = computed(() => Number(props.id))

const toast = useToast()
const { t } = useI18n()

const userStore = useUserStore()

const { clan, loadClan, loadingClan } = useClan(clanId)

const { clanMembers, loadClanMembers, getClanMember } = useClanMembers(clanId)

const {
  borrowItem,
  clanArmory,
  isLoadingClanArmory,
  loadClanArmory,
  removeItem,
  returnItem,
  getClanArmoryItem,
} = useClanArmory(clanId)

const refreshData = () => Promise.all([
  userStore.fetchUser(),
  userStore.fetchUserItems(),
  loadClanArmory(),
])

const {
  execute: onBorrowFromClanArmory,
} = useAsyncCallback(async (userItemId: number) => {
  await borrowItem(userItemId)
  await refreshData()
  toast.add({
    title: t('clan.armory.item.borrow.notify.success'),
    close: false,
    color: 'success',
  })
})

const {
  execute: onRemoveFromClanArmory,
} = useAsyncCallback(async (userItemId: number) => {
  await removeItem(userItemId)
  await refreshData()
  toast.add({
    title: t('clan.armory.item.remove.notify.success'),
    close: false,
    color: 'success',
  })
})

const {
  execute: onReturnFromClanArmory,
} = useAsyncCallback(async (userItemId: number) => {
  await returnItem(userItemId)
  await refreshData()
  toast.add({
    title: t('clan.armory.item.return.notify.success'),
    close: false,
    color: 'success',
  })
})

const hideOwnedItemsModel = useStorage<boolean>('clan-armory-hide-owned-items', true)
const showOnlyAvailableItems = useStorage<boolean>('clan-armory-show-only-available-items', true)
const additionalFilteritems = computed<DropdownMenuItem[]>(() => [
  {
    label: t('clan.armory.filter.hideOwned'),
    type: 'checkbox' as const,
    checked: hideOwnedItemsModel.value,
    onUpdateChecked(checked: boolean) {
      hideOwnedItemsModel.value = checked
    },
  },
  {
    label: t('clan.armory.filter.showOnlyAvailable'),
    type: 'checkbox' as const,
    checked: showOnlyAvailableItems.value,
    onUpdateChecked(checked: boolean) {
      showOnlyAvailableItems.value = checked
    },
  },
])

const items = computed(() => {
  if (!clanArmory.value.length || !userStore.user) {
    return []
  }
  return clanArmory.value.filter((armoryItem) => {
    // Hide owned items if enabled
    if (hideOwnedItemsModel.value && isOwnClanArmoryItem(armoryItem, userStore.user!.id)) {
      return false
    }
    // Show only available items if enabled
    if (showOnlyAvailableItems.value) {
      // Item is available if not borrowed, not owned, and not in user's inventory
      if (
        armoryItem.borrowerUserId
        || isOwnClanArmoryItem(armoryItem, userStore.user!.id)
        || isClanArmoryItemInInventory(armoryItem, userStore.userItems)
      ) {
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

Promise.all([
  userStore.fetchUserItems(),
  loadClan(),
  loadClanArmory(),
  loadClanMembers(),
])
</script>

<template>
  <UContainer class="space-y-6 py-6">
    <AppBackButton :to="{ name: 'clans-id', params: { id: clanId } }" data-aq-link="back-to-clan" />

    <div class="mx-auto max-w-2xl">
      <UiHeading
        :title="$t('clan.armory.title')"
        class="mb-14"
      />

      <ItemGrid
        v-if="Boolean(items.length)"
        v-model:sorting="sortingModel"
        :items="items"
        :sorting-config="sortingConfig"
      >
        <template #filter-leading>
          <UDropdownMenu
            size="xl"
            :items="additionalFilteritems"
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
          @borrow="(id) => {
            closeItemDetail(di);
            onBorrowFromClanArmory(id);
          }"
          @remove="(id) => {
            closeItemDetail(di);
            onRemoveFromClanArmory(id);
          }"
          @return="(id) => {
            closeItemDetail(di);
            onReturnFromClanArmory(id);
          }"
        />
      </template>
    </ItemDetailGroup>
  </UContainer>
</template>
