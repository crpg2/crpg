<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import type { Character } from '~/models/character'

import { characterClassToIcon } from '~/services/character-service'

const props = defineProps<{
  characters: Character[]
  currentCharacter: Character
  activeCharacterId: number | null
}>()

const emit = defineEmits<{
  activate: [number, boolean]
  create: []
}>()

const { t } = useI18n()

const items = computed<DropdownMenuItem[][]>(() => [
  [
    ...props.characters.map<DropdownMenuItem>(character => ({
      checked: character.id === props.currentCharacter.id,
      type: 'link',
      icon: `crpg:${characterClassToIcon[character.class]}`,
      label: character.name,
      to: { name: 'characters-id', params: { id: character.id } },
      slot: 'character' as const,
    })),
    {
      label: t('character.create.title'),
      icon: 'crpg:plus',
      color: 'primary',
      onSelect: () => {
        emit('create')
      },
    },
  ],

])
</script>

<template>
  <div>
    <UDropdownMenu
      :items="items"
      size="xl"
      :ui="{
        content: 'w-48',
      }"
    >
      <UButton
        variant="outline"
        size="xl"
      >
        <CharacterMedia
          :character="currentCharacter"
          :is-active="currentCharacter.id === activeCharacterId"
        />

        <USeparator orientation="vertical" />

        <UIcon
          name="crpg:chevron-down"
          class="size-5 text-dimmed  duration-200 group-data-[state=open]:rotate-180"
        />
      </UButton>

      <!-- <template #character="item">
        <pre>{{ item }}</pre>
      </template> -->
    </UDropdownMenu>

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
  </div>
</template>
