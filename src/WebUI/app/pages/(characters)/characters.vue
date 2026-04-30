<script setup lang="ts">
import { useCharactersProvider } from '~/composables/character/use-character'
import { SomeRole } from '~/models/role'

definePageMeta({
  roles: SomeRole,
  middleware: [
    async () => {
      // TODO: FIXME: два раза при SPA
      const { execute } = useCharactersProvider()
      await execute()
    },
  ],
})
</script>

<template>
  <UContainer class="py-6">
    <NuxtPage />
  </UContainer>
</template>
