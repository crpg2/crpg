<script setup lang="ts">
import { SomeRole } from '~/models/role'
import { getSelfUpdate, registerParty } from '~/services/strategus/party-service'

// import { useCharacters } from '~/composables/character/use-character'

definePageMeta({
  layoutOptions: {
    bg: 'background-2.webp',
  },
  roles: SomeRole,
  middleware: [
    async () => {
      try {
        const partyRes = await getSelfUpdate()

        if (partyRes?.errors === null && partyRes.data !== null) {
          return navigateTo({ name: 'strategus' })
        }
      }
      // eslint-disable-next-line unused-imports/no-unused-vars
      catch (_error) {
      }
    },
  ],
})

const [join, joining] = useAsyncCallback(async () => {
  await registerParty()
  navigateTo({ name: 'strategus' })
})
</script>

<template>
  <div class="flex min-h-125 items-center justify-center">
    <div class="space-y-8">
      <UiDecorSeparator />
      <div class="text-center">
        <UiTextView variant="h3" tag="h3" margin-bottom>
          JOIN TO STRAT TODO:
        </UiTextView>
        <UiTextView variant="p" tag="p">
          bla bla bla
        </UiTextView>
        <UButton :loading="joining" @click="join">
          Join
        </UButton>
      </div>
      <UiDecorSeparator />
    </div>
  </div>
</template>
