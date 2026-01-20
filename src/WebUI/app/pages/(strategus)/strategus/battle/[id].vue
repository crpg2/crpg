<script setup lang="ts">
import { usePartyState } from '~/composables/strategus/use-party'
import { useSettlement } from '~/composables/strategus/use-settlements'
import { PARTY_STATUS } from '~/models/strategus/party'

definePageMeta({
  middleware: [
    () => {
      // TODO: validation
      const { party } = usePartyState().value
      if (!party.battleId && party.status !== PARTY_STATUS.InBattle) {
        return navigateTo({ name: 'strategus' })
      }
    },
  ],
})
</script>

<template>
  <UCard
    variant="subtle"
  >
    <div class="h-full overflow-x-hidden overflow-y-auto">
      <RouterView />
    </div>

    <footer class="border-border-200 flex items-center gap-5 border-t pt-2">
      TODO:
    </footer>
  </UCard>
</template>
