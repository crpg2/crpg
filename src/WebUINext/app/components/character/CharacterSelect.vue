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
      icon: `crpg:${characterClassToIcon[character.class]}`,
      label: character.name,
      to: { params: { id: character.id } },
      slot: 'character' as const,
      character,
    })),
  ],
  [
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
  <UDropdownMenu
    :items="items"
    :modal="false"
    size="xl"
  >
    <UButton
      variant="outline"
      size="xl"
    >
      <CharacterMedia :character="currentCharacter" />

      <UTooltip v-if="!activeCharacterId" :text="$t('character.noneSomeActive.title')">
        <UBadge
          :label="$t('character.noneSomeActive.short')"
          color="warning"
          variant="soft"
          size="sm"
          icon="crpg:alert"
        />
      </UTooltip>

      <UTooltip v-else-if="currentCharacter.id === activeCharacterId" :text="$t('character.status.active.title')">
        <UBadge
          :label="$t('character.status.active.short')"
          color="success"
          variant="soft"
          size="sm"
        />
      </UTooltip>

      <UTooltip v-else :text="$t('character.status.inactive.title')">
        <UBadge
          :label="$t('character.status.inactive.short')"
          color="neutral"
          variant="soft"
          size="sm"
        />
      </UTooltip>

      <USeparator orientation="vertical" />

      <UIcon
        name="crpg:chevron-down"
        class="
          size-5 text-dimmed duration-200
          group-data-[state=open]:rotate-180
        "
      />
    </UButton>

    <template #character="{ item }: { item: { character:Character } }">
      <CharacterSelectItem
        :model-value="activeCharacterId === item.character.id"
        :character="(item.character as Character)"
        @update:model-value="(val) => $emit('activate', item.character.id, val)"
      />
    </template>
  </UDropdownMenu>
</template>
