<script setup lang="ts">
import NumberFlow from '@number-flow/vue'

import type { CharacterCharacteristics, CharacteristicKey, CharacteristicSectionKey, SkillKey } from '~/models/character'

import { ATTRIBUTES_TO_SKILLS_RATE, characteristicBonusByKey, SKILLS_TO_ATTRIBUTES_RATE } from '~/services/character-service'

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
    v-for="({ key: fieldGroupKey, children }) in formSchema"
    :key="fieldGroupKey"
    :ui="{ body: '!p-0 overflow-hidden', header: '!px-4 py-3' }"
    :style="{ 'grid-area': fieldGroupKey }"
  >
    <template #header>
      <UiDataCell
        :data-aq-fields-group="fieldGroupKey"
      >
        <UiTextView variant="p">
          {{ $t(`character.characteristic.${fieldGroupKey}.title`) }} -

          <span
            class="font-bold"
            :class="[characteristics[fieldGroupKey].points < 0 ? `text-error` : `text-success`]"
          >
            <NumberFlow :value="characteristics[fieldGroupKey].points" />
          </span>
        </UiTextView>

        <template #rightContent>
          <UTooltip v-if="fieldGroupKey === 'attributes'">
            <UButton
              variant="outline"
              color="neutral"
              :disabled="convertAttributesToSkillsState.disabled"
              :loading="convertAttributesToSkillsState.loading"
              :label="convertAttributesToSkillsState.count !== undefined ? String(convertAttributesToSkillsState.count) : undefined"
              icon="crpg:convert"
              data-aq-convert-attributes-action
              @click="$emit('convertAttributesToSkills')"
            />
            <template #content>
              <UiTooltipContent
                :title="$t('character.characteristic.convert.attrsToSkills.title')"
              >
                <template #description>
                  <i18n-t
                    scope="global"
                    keypath="character.characteristic.convert.attrsToSkills.tooltip"
                    tag="p"
                  >
                    <template #attribute>
                      <span class="font-bold text-error">{{ ATTRIBUTES_TO_SKILLS_RATE }}</span>
                    </template>
                    <template #skill>
                      <span class="font-bold text-success">{{ SKILLS_TO_ATTRIBUTES_RATE }}</span>
                    </template>
                  </i18n-t>
                </template>
              </UiTooltipContent>
            </template>
          </UTooltip>

          <UTooltip v-else-if="fieldGroupKey === 'skills'">
            <UButton
              variant="outline"
              color="neutral"
              :disabled="convertSkillsToAttributesState.disabled"
              :loading="convertSkillsToAttributesState.loading"
              :label="convertSkillsToAttributesState.count !== undefined ? String(convertSkillsToAttributesState.count) : undefined"
              icon="crpg:convert"
              data-aq-convert-skills-action
              @click="$emit('convertSkillsToAttributes')"
            />
            <template #content>
              <UiTooltipContent
                :title="$t('character.characteristic.convert.skillsToAttrs.title')"
              >
                <template #description>
                  <i18n-t
                    scope="global"
                    keypath="character.characteristic.convert.skillsToAttrs.tooltip"
                    tag="p"
                  >
                    <template #attribute>
                      <span class="font-bold text-success">{{ ATTRIBUTES_TO_SKILLS_RATE }}</span>
                    </template>
                    <template #skill>
                      <span class="font-bold text-error">{{ SKILLS_TO_ATTRIBUTES_RATE }}</span>
                    </template>
                  </i18n-t>
                </template>
              </UiTooltipContent>
            </template>
          </UTooltip>
        </template>
      </UiDataCell>
    </template>

    <UiDataCell
      v-for="({ key: fieldKey }) in children"
      :key="fieldKey"
      class="
        w-full px-4 py-2.5
        hover:bg-muted
      "
    >
      <UTooltip :content="{ side: 'top' }">
        <div
          class="flex items-center gap-1 text-sm"
          :class="{
            'text-error': fieldGroupKey === 'skills' && !checkCurrentSkillRequirementsSatisfied(fieldKey as SkillKey),
          }"
        >
          <UiTextView variant="caption">
            {{ $t(`character.characteristic.${fieldGroupKey}.children.${fieldKey}.title`) }}
          </UiTextView>

          <UIcon
            v-if="fieldGroupKey === 'skills' && !checkCurrentSkillRequirementsSatisfied(fieldKey as SkillKey)"
            name="crpg:alert-circle"
            class="size-4"
          />
        </div>

        <template #content>
          <UiTooltipContent
            :title="$t(`character.characteristic.${fieldGroupKey}.children.${fieldKey}.title`)"
          >
            <template v-if="$t(`character.characteristic.${fieldGroupKey}.children.${fieldKey}.requires`)" #validation>
              <UiTextView variant="p" class="text-warning">
                {{ $t('character.characteristic.requires.title', { text: $t(`character.characteristic.${fieldGroupKey}.children.${fieldKey}.requires`) }) }}
              </UiTextView>
            </template>

            <template v-if="$t(`character.characteristic.${fieldGroupKey}.children.${fieldKey}.desc`)" #description>
              <i18n-t
                scope="global"
                :keypath="`character.characteristic.${fieldGroupKey}.children.${fieldKey}.desc`"
              >
                <template
                  v-if="characteristicBonusByKey[fieldKey]"
                  #value
                >
                  <span class="font-bold text-success">
                    {{ $n(characteristicBonusByKey[fieldKey].value, { style: characteristicBonusByKey[fieldKey].style, minimumFractionDigits: 0 }) }}
                  </span>
                </template>
              </i18n-t>
            </template>
          </UiTooltipContent>
        </template>
      </UTooltip>

      <template #rightContent>
        <UInputNumber
          :data-aq-control="`${fieldGroupKey}:${fieldKey}`"
          variant="outline"
          size="lg"
          :color="fieldGroupKey === 'skills' && !checkCurrentSkillRequirementsSatisfied(fieldKey as SkillKey) ? 'error' : 'neutral'"
          :ui="{
            base: 'w-28',
          }"
          v-bind="getInputProps(fieldGroupKey, fieldKey)"
          @update:model-value="(value) => $emit('input', fieldGroupKey, fieldKey, value)"
        />
      </template>
    </UiDataCell>
  </UCard>
</template>
