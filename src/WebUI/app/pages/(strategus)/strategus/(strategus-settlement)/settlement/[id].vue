<script setup lang="ts">
import type { RouteLocationNormalizedLoaded } from 'vue-router'

import { usePartyState } from '~/composables/strategus/use-party'
import { useSettlement } from '~/composables/strategus/use-settlements'
import { shouldPartyBeInSettlement } from '~/services/strategus/party-service'

definePageMeta({
  middleware: [
    async (to) => {
      const { partyState } = usePartyState()

      if (!shouldPartyBeInSettlement(partyState.value.party)) {
        return navigateTo({ name: 'strategus' })
      }

      const settlementId = Number((to as RouteLocationNormalizedLoaded<'strategus-settlement-id'>).params.id)

      if (Number.isNaN(settlementId)) {
        return navigateTo({ name: 'strategus' })
      }
    },
  ],
})

// const { user } = toRefs(useUserStore())

// const router = useRouter()

// const { state: settlement, execute: loadSettlement, isLoading: loadingSettlement } = useAsyncState(
//   () => getSettlement(Number(route.params.id)),
//   null,
// )

const { settlement } = useSettlement()

// provide(settlementKey, settlement)

// const { party, moveParty } = injectStrict(partyKey)

// async function leaveFromSettlement(): Promise<void> {
//   await moveParty({
//     status: PartyStatus.Idle,
//   })
//   router.push({
//     name: 'Strategus',
//   })
// }
</script>

<template>
  <UCard
    class=""
    variant="subtle"
  >
    <!-- class="flex h-[95%] w-2/5 flex-col space-y-4 overflow-hidden bg-default/90 p-6 backdrop-blur-sm" -->
    <!-- <header class="border-border-200 border-b pb-2">

      <div class="flex items-center gap-5">
        <OButton
          v-tooltip.bottom="`Leave`"
          variant="secondary"
          size="lg"
          outlined
          rounded
          icon-left="arrow-left"
          @click="leaveFromSettlement"
        />

        <OLoading :active="loadingSettlement" :full-page="false" />
        <div v-if="settlement" class="flex items-center gap-5">
          <SettlementMedia :settlement="settlement" />

          <Divider inline />

          <div v-tooltip.bottom="`Troops`" class="flex items-center gap-1.5">
            <OIcon icon="member" size="lg" />
            {{ settlement.troops }}
          </div>

          <Divider inline />

          <Coin :value="10000" />

          <Divider inline />

          <div v-if="settlement?.owner" class="flex flex-col gap-1">
            <span class="text-3xs text-content-300">Owner</span>
            <UserMedia
              :user="settlement.owner"
              :is-self="settlement.owner.id === user!.id"
              class="max-w-64"
            />
          </div>
        </div>
      </div>
    </header> -->

    <!-- <nav class="flex items-center justify-center gap-2">
      <RouterLink
        v-slot="{ isExactActive }"
        :to="{ name: 'StrategusSettlementId', params: { id: route.params.id } }"
      >
        <OButton
          :variant="isExactActive ? 'transparent-active' : 'secondary'"
          size="sm"
          label="Info"
        />
      </RouterLink>

      <RouterLink
        v-slot="{ isExactActive }"
        :to="{ name: 'StrategusSettlementIdGarrison', params: { id: route.params.id } }"
      >
        <OButton
          :variant="isExactActive ? 'transparent-active' : 'secondary'"
          size="sm"
          icon-left="member"
          label="Garrison"
        />
      </RouterLink>
    </nav> -->

    <div class="h-full overflow-x-hidden overflow-y-auto">
      <NuxtPage />
    </div>

    <footer class="border-border-200 flex items-center gap-5 border-t pt-2">
      TODO:
    </footer>
  </UCard>
</template>
