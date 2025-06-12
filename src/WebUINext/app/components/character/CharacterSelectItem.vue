<script setup lang="ts">
import type { Character } from '~/models/character'

const { character } = defineProps<{
  character: Character
}>()

const modelValue = defineModel<boolean>({ default: false })
</script>

<template>
  <div class="flex items-center gap-2">
    <UTooltip :ui="{ content: 'max-w-72' }" :content="{ side: 'right' }">
      <div @click.prevent.stop>
        <USwitch v-model="modelValue" />
      </div>

      <template #content>
        <div class="prose prose-invert">
          <h5 class="text-content-100">
            {{ $t('character.settings.active.tooltip.title') }}
          </h5>
          <div v-html="$t('character.settings.active.tooltip.desc')" />
        </div>
      </template>
    </UTooltip>

    <CharacterMedia :character />
  </div>
</template>
