<script setup lang="ts">
import type { CharacterCharacteristics, CharacteristicKey, CharacteristicSectionKey, SkillKey } from '~/models/character'

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
  getInputProps: (group: CharacteristicSectionKey, field: CharacteristicKey) => { modelValue: number, min: number, max: number, costToIncrease: number }
  checkCurrentSkillRequirementsSatisfied: (skillKey: SkillKey) => boolean
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
    v-for="({ key: fieldGroupKey, children }) in formSchema"
    :key="fieldGroupKey"
    :ui="{ body: 'p-0! overflow-hidden', header: 'px-4! py-3' }"
    :style="{ 'grid-area': fieldGroupKey }"
  >
    <template #header>
      <CharacterCharacteristicsBuilderGroupHeader
        :field-group-key
        :points="characteristics[fieldGroupKey].points"
        :convert-attributes-to-skills-state
        :convert-skills-to-attributes-state
        @convert-attributes-to-skills="$emit('convertAttributesToSkills')"
        @convert-skills-to-attributes="$emit('convertSkillsToAttributes')"
      />
    </template>

    <CharacterCharacteristicsBuilderField
      v-for="({ key: fieldKey }) in children"
      :key="fieldKey"
      :field-group-key
      :field-key
      :is-error="fieldGroupKey === 'skills' && !checkCurrentSkillRequirementsSatisfied(fieldKey as SkillKey)"
      :input-props="getInputProps(fieldGroupKey, fieldKey)"
      @update:model-value="$emit('inputWithAutoClamp', fieldGroupKey, fieldKey, $event)"
      @fill-field="$emit('fillField', fieldGroupKey, fieldKey)"
      @reset-field="$emit('resetField', fieldGroupKey, fieldKey)"
    />
  </UCard>
</template>
