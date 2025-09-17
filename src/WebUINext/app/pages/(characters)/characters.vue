<script setup lang="ts">
import { usePollInterval } from '~/composables/utils/use-poll-interval'
import { SomeRole } from '~/models/role'
import { pollUserCharactersSymbol } from '~/symbols'

definePageMeta({
  roles: SomeRole,
  middleware: [
    /**
     * @description
     * load characters
     */
    async () => {
      const userStore = useUserStore()
      if (!userStore.characters.length) {
        await userStore.fetchCharacters()
      }
    },
  ],
})

const userStore = useUserStore()

usePollInterval({
  key: pollUserCharactersSymbol,
  fn: userStore.fetchCharacters,
})
</script>

<template>
  <UContainer class="py-6">
    <NuxtPage />
  </UContainer>
</template>
