<script setup lang="ts">
import type { Character } from '~/models/character'

defineProps<{
  character: Character
  isSelected: boolean
}>()

const modelValue = defineModel<boolean>({ default: false })
</script>

<template>
  <div class="flex items-center gap-2">
    <UTooltip :content="{ side: 'right' }">
      <div @click.prevent.stop>
        <USwitch v-model="modelValue" />
      </div>

      <template #content>
        <UiTooltipContent :title="$t('character.settings.active.tooltip.title')">
          <template #description>
            <p v-for="(p, idx) in $tm('character.settings.active.tooltip.desc')" :key="idx">
              {{ $rt(p) }}
            </p>
          </template>
        </UiTooltipContent>
      </template>
    </UTooltip>

    <CharacterMedia :character :class="{ 'text-primary': isSelected }" />
  </div>
</template>
