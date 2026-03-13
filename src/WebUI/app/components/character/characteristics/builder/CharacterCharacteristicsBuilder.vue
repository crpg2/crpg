<script setup lang="ts">
import type { CharacterCharacteristics, CharacteristicKey, CharacteristicSectionKey } from '~/models/character'
import type { CharacteristicProps } from '~/services/character-service'

interface FormSchema {
  key: CharacteristicSectionKey
  children: {
    key: CharacteristicKey
  }[]
}

interface ConvertState {
  disabled: boolean
  loading?: boolean
  count?: number
}

defineProps<{
  getInputProps: (group: CharacteristicSectionKey, field: CharacteristicKey) => CharacteristicProps
  characteristics: CharacterCharacteristics
  convertAttributesToSkillsState: ConvertState
  convertSkillsToAttributesState: ConvertState
}>()

defineEmits<{
  input: [groupKey: CharacteristicSectionKey, fieldKey: CharacteristicKey, value: number]
  inputWithAutoClamp: [groupKey: CharacteristicSectionKey, fieldKey: CharacteristicKey, value: number]
  convertAttributesToSkills: []
  convertSkillsToAttributes: []
  fillField: [groupKey: CharacteristicSectionKey, fieldKey: CharacteristicKey]
  resetField: [groupKey: CharacteristicSectionKey, fieldKey: CharacteristicKey]
}>()

const formSchema: FormSchema[] = [
  {
    key: 'attributes',
    children: [
      {
        key: 'strength',
      },
      {
        key: 'agility',
      },
    ],
  },
  {
    key: 'skills',
    children: [
      {
        key: 'ironFlesh',
      },
      {
        key: 'powerStrike',
      },
      {
        key: 'powerDraw',
      },
      {
        key: 'powerThrow',
      },
      {
        key: 'athletics',
      },
      {
        key: 'riding',
      },
      {
        key: 'weaponMaster',
      },
      {
        key: 'mountedArchery',
      },
      {
        key: 'shield',
      },
    ],
  },
  {
    key: 'weaponProficiencies',
    children: [
      {
        key: 'oneHanded',
      },
      {
        key: 'twoHanded',
      },
      {
        key: 'polearm',
      },
      {
        key: 'bow',
      },
      {
        key: 'crossbow',
      },
      {
        key: 'throwing',
      },
    ],
  },
]
</script>

<template>
  <UCard
    v-for="({ key: section, children }) in formSchema"
    :key="section"
    :ui="{ body: 'p-0! overflow-hidden', header: 'px-4! py-3' }"
    :style="{ 'grid-area': section }"
  >
    <template #header>
      <CharacterCharacteristicsBuilderGroupHeader
        :data-aq-fields-group="section"
        :section
        :points="characteristics[section].points"
        :convert-attributes-to-skills-state
        :convert-skills-to-attributes-state
        @convert-attributes-to-skills="$emit('convertAttributesToSkills')"
        @convert-skills-to-attributes="$emit('convertSkillsToAttributes')"
      />
    </template>

    <CharacterCharacteristicsBuilderField
      v-for="({ key: characteristic }) in children"
      :key="characteristic"
      :section
      :characteristic
      :input-props="getInputProps(section, characteristic)"
      @update:model-value="$emit('inputWithAutoClamp', section, characteristic, $event)"
      @fill-field="$emit('fillField', section, characteristic)"
      @reset-field="$emit('resetField', section, characteristic)"
    />
  </UCard>
</template>
