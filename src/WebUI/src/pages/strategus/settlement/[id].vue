<script setup lang="ts">
import { PartyStatus } from '~/models/strategus/party'
import { SettlementType } from '~/models/strategus/settlement'
import { getSettlement } from '~/services/strategus-service/settlement'
import { useUserStore } from '~/stores/user'
import { partyKey, settlementKey } from '~/symbols/strategus'

// TODO: middleware
definePage({
  meta: {
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const { user } = toRefs(useUserStore())

const route = useRoute<'StrategusSettlementId'>()
const router = useRouter()

const { state: settlement, execute: loadSettlement, isLoading: loadingSettlement } = useAsyncState(
  () => getSettlement(Number(route.params.id)),
  null,
)

provide(settlementKey, settlement)

const { party, moveParty } = injectStrict(partyKey)

async function leaveFromSettlement(): Promise<void> {
  await moveParty({
    status: PartyStatus.Idle,
  })
  router.push({
    name: 'Strategus',
  })
}
</script>

<template>
  <div
    class="flex h-[95%] w-2/5 flex-col space-y-4 overflow-hidden rounded-3xl bg-base-100/90 p-6 text-content-200 backdrop-blur-sm"
  >
    <header class="border-b border-border-200 pb-2">
      <!-- TODO: leave from city logic, API -->
      <!-- Exit gate/door icon -->

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

          <!-- TODO: gold? -->
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
    </header>

    <nav class="flex items-center justify-center gap-2">
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
    </nav>

    <div class="h-full overflow-y-auto overflow-x-hidden">
      <RouterView />
    </div>

    <footer class="flex items-center gap-5 border-t border-border-200 pt-2">
      TODO:
    </footer>
  </div>
</template>
