<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { usePartyState } from '~/composables/strategus/use-party'
import { useSettlement, useSettlementProvider } from '~/composables/strategus/use-settlements'
import { useUser } from '~/composables/user/use-user'
import { shouldPartyBeInSettlement } from '~/services/strategus/party-service'
import { checkCanEditSettlementInventory } from '~/services/strategus/settlement-service'

definePageMeta({
  middleware: [
    async (to) => {
      const { partyState } = usePartyState()

      const settlementId = Number((to as RouteLocationNormalizedLoaded<'strategus-settlement-id'>).params.id)

      if (!shouldPartyBeInSettlement(partyState.value.party) || partyState.value.party.currentSettlement?.id !== settlementId) {
        return navigateTo({ name: 'strategus' })
      }

      if (Number.isNaN(settlementId)) {
        return navigateTo({ name: 'strategus' })
      }

      const { data: settlement, error } = await useSettlementProvider(settlementId)

      if (!settlement.value || error.value) {
        return navigateTo({ name: 'strategus' })
      }
    },
  ],
})

// const router = useRouter()

const { settlement } = useSettlement()

// async function leaveFromSettlement(): Promise<void> {
//   await moveParty({
//     status: PartyStatus.Idle,
//   })
//   router.push({
//     name: 'Strategus',
//   })
// }

const { user } = useUser()

const route = useRoute<'strategus-settlement-id'>()

const navigationItems = computed<NavigationMenuItem[]>(() => [
  {
    label: 'Town center',
    to: { name: 'strategus-settlement-id', params: { id: route.params.id } },
    active: route.name === 'strategus-settlement-id', // hack, [id].vue conflict with [id]/index.vue
  },
  ...(checkCanEditSettlementInventory(settlement.value, user.value!)
    ? [
        {
          label: 'Garrison/Inventory',
          to: { name: 'strategus-settlement-id-inventory', params: { id: route.params.id } },
          active: route.name === 'strategus-settlement-id-inventory', // hack, [id].vue conflict with [id]/index.vue
          icon: 'crpg:chest',
        } as NavigationMenuItem,
      ]
    : []),

])
</script>

<template>
  <MapSidePage class="w-4xl">
    <template #header>
      <div class="flex flex-wrap items-center gap-8">
        <SettlementMedia :settlement size="xl" />

        <UiDataContent
          v-if="settlement.owner"
          caption="Owner"
          layout="reverse"
          size="sm"
        >
          <UserMedia
            v-if="settlement.owner"
            size="sm"
            :user="settlement.owner"
            :is-self="settlement.owner.id === user!.id"
            class="max-w-64"
          />
        </UiDataContent>

        <div class="flex flex-wrap items-center gap-4">
          <UBadge icon="crpg:region" :label="$t(`region.${settlement.region}`, 0)" size="lg" variant="soft" color="neutral" />

          <UTooltip>
            <AppCoin :value="500000" compact />
            <template #content>
              <AppCoin :value="500000" />
            </template>
          </UTooltip>

          <UTooltip>
            <UiDataMedia icon="crpg:member" :label="$n(settlement.troops, 'compact')" />
            <template #content>
              <UiDataMedia icon="crpg:member" :label="$n(settlement.troops)" />
            </template>
          </UTooltip>
        </div>
      </div>
    </template>

    <UNavigationMenu
      variant="pill"
      color="neutral"
      class="mb-2 flex w-full justify-center gap-4"
      :items="navigationItems"
    />

    <NuxtPage />
  </MapSidePage>
</template>
