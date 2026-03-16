<script setup lang="ts">
import NumberFlow from '@number-flow/vue'

import type { CharacteristicSectionKey } from '~/models/character'

import { ATTRIBUTES_TO_SKILLS_RATE, SKILLS_TO_ATTRIBUTES_RATE } from '~/services/character-service'

interface ConvertState {
  disabled: boolean
  loading?: boolean
  count?: number
}

defineProps<{
  section: CharacteristicSectionKey
  points: number
  convertAttributesToSkillsState?: ConvertState
  convertSkillsToAttributesState?: ConvertState
}>()

defineEmits<{
  convertAttributesToSkills: []
  convertSkillsToAttributes: []
}>()
</script>

<template>
  <UiDataCell>
    <UiTextView variant="p">
      {{ $t(`character.characteristic.${section}.title`) }} -

      <span
        class="font-bold"
        :class="[points < 0 ? 'text-error' : 'text-success']"
      >
        <NumberFlow :value="points" />
      </span>
    </UiTextView>

    <template #rightContent>
      <UTooltip v-if="section === 'attributes' && convertAttributesToSkillsState">
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

      <UTooltip v-else-if="section === 'skills' && convertSkillsToAttributesState">
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
