<script setup lang="ts">
import type { Character } from '~/models/character'

const { characters } = defineProps<{
  characters: Character[]
  currentCharacter: Character
  activeCharacterId: number | null
}>()

defineEmits<{
  activate: [number, boolean]
  create: []
}>()
</script>

<template>
  <VDropdown
    :triggers="['click']"
    placement="bottom-end"
  >
    <template #default="{ shown }">
      <OButton
        variant="primary"
        outlined
        size="lg"
      >
        <CharacterMedia
          :character="currentCharacter"
          :is-active="currentCharacter.id === activeCharacterId"
        />

        <UiDivider inline />

        <OIcon
          icon="chevron-down"
          size="lg"
          :rotation="shown ? 180 : 0"
          class="text-content-400"
        />
      </OButton>
    </template>

    <template #popper="{ hide }">
      <div class="min-w-96">
        <UiDropdownItem
          v-for="char in characters"
          :key="char.id"
          :checked="char.id === currentCharacter.id"
          :link="{ to: { name: 'characters-id', params: { id: char.id } } }"
          @click="hide"
        >
          <CharacterSelectItem
            :character="char"
            :model-value="activeCharacterId === char.id"
            @update:model-value="(val) => $emit('activate', char.id, val)"
          />
        </UiDropdownItem>

        <UiDropdownItem
          class="text-primary hover:text-primary-hover"
          @click="
            () => {
              $emit('create')
              hide();
            }
          "
        >
          <OIcon
            icon="add"
            size="lg"
          />
          {{ $t('character.create.title') }}
        </UiDropdownItem>
      </div>
    </template>
  </VDropdown>
</template>
