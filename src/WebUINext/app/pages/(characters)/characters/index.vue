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
    () => {
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
      <div class="text-center">
        <UiTextView variant="h3" tag="h3" margin-bottom>
          {{ $t('character.empty.title') }}
        </UiTextView>
        <UiTextView variant="p" tag="p">
          {{ $t('character.empty.desc') }}
        </UiTextView>
      </div>
      <UiDecorSeparator />
    </div>
  </div>
</template>
