<script setup lang="ts">
import type { Character } from '~/models/character'

const { character } = defineProps<{
  character: Character
}>()

const modelValue = defineModel<boolean>({ default: false })
</script>

<template>
  <div class="flex flex-1 items-center gap-2">
    <VTooltip placement="auto">
      <div @click.stop>
        <OSwitch v-model="modelValue" />
      </div>
      <template #popper>
        <div class="prose prose-invert">
          <h5 class="text-content-100">
            {{ $t('character.settings.active.tooltip.title') }}
          </h5>
          <div v-html="$t('character.settings.active.tooltip.desc')" />
        </div>
      </template>
    </VTooltip>

    <CharacterMedia
      :character="character"
      :is-active="modelValue!"
    />
  </div>
</template>
