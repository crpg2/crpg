<script setup lang="ts">
import { LazySettlementManageItemsDrawer, LazySettlementManageTroopsDrawer } from '#components'

import { useParty, usePartyItems } from '~/composables/strategus/use-party'
import { useSettlement, useSettlementItems } from '~/composables/strategus/use-settlements'
import { useUser } from '~/composables/user/use-user'
import { checkCanEditSettlementInventory } from '~/services/strategus/settlement-service'

definePageMeta({
  middleware: [
    () => {
      const { settlement } = useSettlement()
      const { user } = useUser()

      if (!checkCanEditSettlementInventory(settlement.value, user.value!)) {
        return navigateTo({ name: 'strategus-settlement-id', params: { id: settlement.value.id } })
      }
    },
  ],
})

const route = useRoute<'strategus-settlement-id-inventory'>()

const toast = useToast()

const {
  settlement,
  refreshSettlement,
  updateSettlementResources,
} = useSettlement()

const {
  settlementItems,
  pendingSettlementItems,
  loadSettlementItems,
  updateSettlementItems,
} = useSettlementItems()

const { updateParty } = useParty()
const { loadpartyItems } = usePartyItems()

const overlay = useOverlay()

const openSettlementManageItemsDrawer = () => {
  overlay.create(LazySettlementManageItemsDrawer).open({
    settlementItems: settlementItems.value,
    settlement: settlement.value,
    async onClose(_result, items) {
      if (!_result || !items) {
        return
      }

      await updateSettlementItems(items)

      await Promise.all([
        loadSettlementItems(),
        loadpartyItems(),
      ])

      toast.add({
        title: 'Settlement items updated',
        color: 'success',
      })
    },
  })
}

const openSettlementManageTroopsDrawer = () => {
  overlay.create(LazySettlementManageTroopsDrawer).open({
    settlement: settlement.value,
    async onClose(_result, troops) {
      if (!_result || troops === undefined) {
        return
      }

      await updateSettlementResources(troops)
      await Promise.all([
        refreshSettlement(),
        updateParty(),
      ])

      toast.add({
        title: 'Settlement troops updated',
        color: 'success',
      })
    },
  })
}
</script>

<template>
  <div>
    <ItemStackGrid :items="settlementItems">
      <!-- <template #filter-trailing>
        <UButton
          variant="subtle"
          label="Manage"
          size="lg"
          @click="openSettlementItemsManageDrawer"
        />
      </template> -->
    </ItemStackGrid>

    <UButton
      variant="subtle"
      label="Manage items"
      size="lg"
      @click="openSettlementManageItemsDrawer"
    />

    <UButton
      variant="subtle"
      label="Manage troops"
      size="lg"
      @click="openSettlementManageTroopsDrawer"
    />
  </div>
</template>
