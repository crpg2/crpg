<script setup lang="ts">
import type { CharacterCharacteristics, CharacteristicKey, CharacteristicSectionKey, SkillKey } from '~/models/character'

import { characteristicBonusByKey } from '~/services/character-service'

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
  getInputProps: (group: CharacteristicSectionKey, field: CharacteristicKey) => { modelValue: number, min: number, max: number }
  checkCurrentSkillRequirementsSatisfied: (skillKey: SkillKey) => boolean
  characteristics: CharacterCharacteristics
  convertAttributesToSkillsState: ConvertState
  convertSkillsToAttributesState: ConvertState
}>()

defineEmits<{
  input: [groupKey: CharacteristicSectionKey, fieldKey: CharacteristicKey, value: number]
  convertAttributesToSkills: []
  convertSkillsToAttributes: []
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
    v-for="fieldGroup in formSchema"
    :key="fieldGroup.key"
    :ui="{ root: 'overflow-hidden', body: '!p-0 overflow-hidden', header: '!px-4 py-3' }"
    :style="{ 'grid-area': fieldGroup.key }"
  >
    <template #header>
      <UiDataCell :data-aq-fields-group="fieldGroup.key" class="w-full text-sm">
        {{ $t(`character.characteristic.${fieldGroup.key}.title`) }} - <span class="font-bold" :class="[characteristics[fieldGroup.key].points < 0 ? 'text-status-danger' : 'text-status-success']">{{ characteristics[fieldGroup.key].points }}</span>
        <template #rightContent>
          <UTooltip v-if="fieldGroup.key === 'attributes'">
            <UButton
              variant="outline"
              size="xs"
              :disabled="convertAttributesToSkillsState.disabled"
              :loading="convertAttributesToSkillsState.loading"
              :label="convertAttributesToSkillsState.count !== undefined ? String(convertAttributesToSkillsState.count) : ''"
              icon="crpg:convert"
              data-aq-convert-attributes-action
              @click="$emit('convertAttributesToSkills')"
            />
            <template #content>
              <div class="prose prose-invert">
                <h4>
                  {{ $t('character.characteristic.convert.attrsToSkills.title') }}
                </h4>
                <i18n-t
                  scope="global"
                  keypath="character.characteristic.convert.attrsToSkills.tooltip"
                  class="text-content-200"
                  tag="p"
                >
                  <template #attribute>
                    <span class="font-bold text-status-danger">1</span>
                  </template>
                  <template #skill>
                    <span class="font-bold text-status-success">2</span>
                  </template>
                </i18n-t>
              </div>
            </template>
          </UTooltip>

          <UTooltip v-else-if="fieldGroup.key === 'skills'">
            <UButton
              variant="outline"
              size="xs"
              :disabled="convertSkillsToAttributesState.disabled"
              :loading="convertSkillsToAttributesState.loading"
              :label="convertSkillsToAttributesState.count !== undefined ? String(convertSkillsToAttributesState.count) : ''"
              icon="crpg:convert"
              data-aq-convert-skills-action
              @click="$emit('convertSkillsToAttributes')"
            />
            <template #content>
              <div class="prose prose-invert">
                <h4>
                  {{ $t('character.characteristic.convert.skillsToAttrs.title') }}
                </h4>
                <i18n-t
                  scope="global"
                  keypath="character.characteristic.convert.skillsToAttrs.tooltip"
                  class="text-content-200"
                  tag="p"
                >
                  <template #skill>
                    <span class="font-bold text-status-danger">2</span>
                  </template>
                  <template #attribute>
                    <span class="font-bold text-status-success">1</span>
                  </template>
                </i18n-t>
              </div>
            </template>
          </UTooltip>
        </template>
      </UiDataCell>
    </template>

    <UiDataCell
      v-for="field in fieldGroup.children"
      :key="field.key"
      class="w-full px-4 py-2.5 hover:bg-neutral-900"
    >
      <UTooltip
        :ui="{ content: 'max-w-72' }"
        :content="{
          side: 'top',
        }"
      >
        <div
          class="flex items-center gap-1 text-2xs"
          :class="{
            'text-status-danger': fieldGroup.key === 'skills' && !checkCurrentSkillRequirementsSatisfied(field.key as SkillKey),
          }"
        >
          {{ $t(`character.characteristic.${fieldGroup.key}.children.${field.key}.title`) }}
          <slot name="field-title-trailing" />
          <UIcon
            v-if="fieldGroup.key === 'skills' && !checkCurrentSkillRequirementsSatisfied(field.key as SkillKey)"
            name="crpg:alert-circle"
            class="size-4"
          />
        </div>

        <template #content>
          <div class="prose prose-invert">
            <h4>
              {{ $t(`character.characteristic.${fieldGroup.key}.children.${field.key}.title`) }}
            </h4>

            <i18n-t
              scope="global"
              :keypath="`character.characteristic.${fieldGroup.key}.children.${field.key}.desc`"
              tag="p"
            >
              <template
                v-if="field.key in characteristicBonusByKey"
                #value
              >
                <span class="font-bold text-content-100">
                  {{ $n(characteristicBonusByKey[field.key]!.value, { style: characteristicBonusByKey[field.key]!.style, minimumFractionDigits: 0 }) }}
                </span>
              </template>
            </i18n-t>

            <p
              v-if="$t(`character.characteristic.${fieldGroup.key}.children.${field.key}.requires`)"
              class="text-status-warning"
            >
              {{ $t('character.characteristic.requires.title') }}:
              {{ $t(`character.characteristic.${fieldGroup.key}.children.${field.key}.requires`) }}
            </p>
          </div>
        </template>
      </UTooltip>

      <template #rightContent>
        <UInputNumber
          :data-aq-control="`${fieldGroup.key}:${field.key}`"
          v-bind="getInputProps(fieldGroup.key, field.key)"
          variant="outline"
          :color="fieldGroup.key === 'skills' && !checkCurrentSkillRequirementsSatisfied(field.key as SkillKey) ? 'error' : 'primary'"
          size="sm"
          :ui="{
            base: 'w-28',
          }"
          @update:model-value="(value) => $emit('input', fieldGroup.key, field.key, value)"
        />
      </template>
    </UiDataCell>
  </UCard>
</template>
