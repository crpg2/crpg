<script setup lang="ts">
import { useCharacters } from '~/composables/character/use-character'

definePageMeta({
  layoutOptions: {
    bg: 'background-2.webp',
  },
  middleware: [
    () => {
      const { fallbackCharacterId } = useCharacters()
      if (fallbackCharacterId.value) {
        return navigateTo({
          name: 'characters-id-inventory',
          params: { id: fallbackCharacterId.value },
          replace: true,
        })
      }
    },
  ],
})
</script>

<template>
  <div class="flex min-h-125 items-center justify-center">
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
