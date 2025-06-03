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
      if (userStore.activeCharacterId) {
        return navigateTo({
          name: 'characters-id-inventory',
          params: { id: userStore.activeCharacterId },
        })
      }
    },
  ],
})
</script>

<template>
  <div class="flex min-h-[500px] items-center justify-center">
    <div class="space-y-8">
      <UiDecorSeparator />

      <div class="prose text-center prose-invert">
        <h3 class="text-xl">
          {{ $t('character.empty.title') }}
        </h3>
        <div>{{ $t('character.empty.desc') }}</div>
      </div>

      <UiDecorSeparator />
    </div>
  </div>
</template>
