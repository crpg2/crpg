<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import type { Character } from '~/models/character'

import { characterClassToIcon } from '~/services/character-service'

const { characters, currentCharacterId, readonly = false, simple = true } = defineProps<{
  characters: Character[]
  currentCharacterId: number | null
  activeCharacterId: number | null
  readonly?: boolean
  simple?: boolean
}>()

const emit = defineEmits<{
  select: [number]
  activate: [number, boolean]
  create: []
}>()

const { t } = useI18n()

const items = computed<DropdownMenuItem[][]>(() => [
  [
    ...characters.map<DropdownMenuItem>(character => ({
      icon: `crpg:${characterClassToIcon[character.class]}`,
      label: character.name,
      slot: 'character' as const,
      character,
      onSelect: () => {
        emit('select', character.id)
      },
    })),
  ],
  ...(!simple
    ? [[
        {
          label: t('character.create.title'),
          icon: 'crpg:plus',
          color: 'primary',
          onSelect: () => {
            emit('create')
          },
        } as DropdownMenuItem,
      ]]
    : []),
])

const currentCharacter = computed(() => characters.find(c => c.id === currentCharacterId))
</script>

<template>
  <UDropdownMenu
    :items="items"
    :modal="false"
    size="xl"
    :disabled="readonly"
  >
    <UButton
      variant="outline"
      color="neutral"
      size="xl"
    >
      <template v-if="currentCharacter">
        <CharacterMedia :character="currentCharacter" />

        <UTooltip
          v-if="!activeCharacterId"
          :text="$t('character.noneSomeActive.title')"
        >
          <UBadge
            :label="$t('character.noneSomeActive.short')"
            color="warning"
            variant="soft"
            size="sm"
            icon="crpg:alert"
          />
        </UTooltip>

        <UTooltip
          v-else-if="currentCharacter?.id === activeCharacterId"
          :text="$t('character.status.active.title')"
        >
          <UBadge
            :label="$t('character.status.active.short')"
            color="success"
            variant="soft"
            size="sm"
          />
        </UTooltip>

        <UTooltip
          v-else
          :text="$t('character.status.inactive.title')"
        >
          <UBadge
            :label="$t('character.status.inactive.short')"
            color="neutral"
            variant="soft"
            size="sm"
          />
        </UTooltip>
      </template>

      <UiTextView v-else variant="p">
        {{ $t('character.selectCharacter') }}
      </UiTextView>

      <UIcon
        name="crpg:chevron-down"
        class="
          size-5 text-dimmed duration-200
          group-data-[state=open]:rotate-180
        "
      />
    </UButton>

    <template #character="{ item }: { item: { character: Character } }">
      <CharacterSelectItem
        :simple
        :character="item.character"
        :is-selected="currentCharacter?.id === item.character.id"
        :model-value="activeCharacterId === item.character.id"
        @update:model-value="(val) => $emit('activate', item.character.id, val)"
      />
    </template>
  </UDropdownMenu>
</template>
