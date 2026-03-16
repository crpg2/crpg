<script setup lang="ts">
import { UTooltip } from '#components'

import type { CharacteristicKey, CharacteristicSectionKey } from '~/models/character'
import type { CharacteristicRequirement } from '~/services/character-service'

import { characteristicBonusByKey } from '~/services/character-service'

const { requirement } = defineProps<{
  section: CharacteristicSectionKey
  characteristic: CharacteristicKey
  value: number
  min: number
  max: number
  costToIncrease: number
  requirement: CharacteristicRequirement | null
}>()

defineEmits<{
  'fillField': []
  'resetField': []
  'update:modelValue': [value: number]
}>()

const isError = computed(() => requirement?.satisfied === false)
</script>

<template>
  <UiDataCell
    class="
      w-full px-4 py-2.5
      hover:bg-muted
    "
    :class="[{ 'bg-error/20': isError }]"
  >
    <UTooltip :content="{ side: 'top' }">
      <div
        class="flex items-center gap-1 text-sm"
      >
        <UiTextView variant="caption">
          {{ $t(`character.characteristic.${section}.children.${characteristic}.title`) }}
        </UiTextView>
      </div>

      <template #content>
        <UiTooltipContent :title="$t(`character.characteristic.${section}.children.${characteristic}.title`)">
          <template #validation>
            <UiTextView variant="h4" margin-bottom class="text-warning">
              {{ $t('character.characteristic.requires.title') }}
            </UiTextView>

            <UiTextView variant="p" class="text-warning">
              {{ $t('character.characteristic.requires.costPoints', {
                count: costToIncrease,
                characteristic: $t(`character.characteristic.${section}.genitiveTitle`).toLowerCase(),
              }) }}
            </UiTextView>

            <template v-if="requirement">
              <UiTextView variant="p" class="text-warning">
                {{ $t('character.characteristic.requires.characteristic', {
                  count: requirement.needCharacteristic,
                  characteristic: $t(`character.characteristic.${requirement.section}.children.${requirement.characteristic}.genitiveTitle`, requirement.needCharacteristic).toLowerCase(),
                }) }} ({{ $t('character.characteristic.requires.pointsPerLevel', {
                  count: requirement.characteristicPerLevel,
                  characteristic: $t(`character.characteristic.${requirement.section}.children.${requirement.characteristic}.genitiveTitle`, requirement.characteristicPerLevel).toLowerCase(),
                }) }})
              </UiTextView>
            </template>
          </template>

          <template #description>
            <i18n-t
              scope="global"
              :keypath="`character.characteristic.${section}.children.${characteristic}.desc`"
            >
              <template
                v-if="characteristic in characteristicBonusByKey"
                #value
              >
                <span class="font-bold text-success">
                  {{ $n(characteristicBonusByKey[characteristic]!.value, { style: characteristicBonusByKey[characteristic]!.style, minimumFractionDigits: 0 }) }}
                </span>
              </template>
            </i18n-t>
          </template>
        </UiTooltipContent>
      </template>
    </UTooltip>

    <template #rightContent>
      <UFieldGroup size="sm">
        <UTooltip :text="$t('character.characteristic.resetField')">
          <UButton
            variant="subtle"
            color="neutral"
            icon="crpg:reset"
            :disabled="value <= min"
            :data-aq-reset-field="`${section}:${characteristic}`"
            @click="$emit('resetField')"
          />
        </UTooltip>
        <UInputNumber
          :data-aq-control="`${section}:${characteristic}`"
          variant="subtle"
          :color="isError ? 'error' : 'neutral'"
          :ui="{
            base: 'w-22',
            decrement: 'flex items-center gap-0.5',
          }"
          :model-value="value"
          @update:model-value="$emit('update:modelValue', $event)"
        >
          <template #decrement>
            <UButton
              variant="link"
              color="neutral"
              icon="i-lucide-minus"
              :disabled="value <= min"
            />
          </template>
          <template #increment>
            <UButton
              variant="link"
              color="neutral"
              icon="i-lucide-plus"
              :disabled="max <= value"
            />
          </template>
        </UInputNumber>
        <UTooltip :text="$t('character.characteristic.fillField')">
          <UButton
            variant="subtle"
            color="neutral"
            icon="i-lucide-chevrons-right"
            :disabled="max <= value"
            :data-aq-fill-field="`${section}:${characteristic}`"
            @click="$emit('fillField')"
          />
        </UTooltip>
      </UFieldGroup>
    </template>
  </UiDataCell>
</template>
