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
  <div class="flex size-full min-h-[500px] items-center justify-center">
    <div class="space-y-8">
      <UiDivider />

      <div class="prose text-center prose-invert">
        <h3 class="text-xl text-content-100">
          {{ $t('character.empty.title') }}
        </h3>
        <div>{{ $t('character.empty.desc') }}</div>
      </div>

      <UiDivider />
    </div>
  </div>
</template>
