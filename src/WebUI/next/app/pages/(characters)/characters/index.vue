<script setup lang="ts">
definePageMeta({
  layoutOptions: {
    bg: 'background-2.webp',
  },
  middleware: [
    /**
     * @description
     * redirect to active character
     */
    async () => {
      const userStore = useUserStore()
      if (userStore.characters.length === 0) {
        await userStore.fetchCharacters()
      }

      if (userStore.activeCharacterId) {
        return navigateTo({
          name: 'characters-id',
          params: { id: userStore.activeCharacterId },
        })
      }

      return true
    },
  ],
})
</script>

<template>
  <div class="flex size-full min-h-[500px] items-center justify-center">
    <div class="space-y-8">
      <UiDivider />

      <div class="prose prose-invert text-center">
        <h3 class="text-xl text-content-100">
          {{ $t('character.empty.title') }}
        </h3>
        <div>{{ $t('character.empty.desc') }}</div>
      </div>

      <UiDivider />
    </div>
  </div>
</template>
