<script setup lang="ts">
import { UTooltip } from '#components'

import type { CharacteristicKey, CharacteristicSectionKey } from '~/models/character'

import { characteristicBonusByKey } from '~/services/character-service'

const props = defineProps<{
  fieldGroupKey: CharacteristicSectionKey
  fieldKey: CharacteristicKey
  inputProps: { modelValue: number, min: number, max: number }
  skillRequirementSatisfied: boolean
}>()

defineEmits<{
  'fillField': []
  'resetField': []
  'update:modelValue': [value: number]
}>()

const isError = computed(() => props.fieldGroupKey === 'skills' && !props.skillRequirementSatisfied)
</script>

<template>
  <UiDataCell
    class="
      w-full px-4 py-2.5
      hover:bg-muted
    "
  >
    <UTooltip :content="{ side: 'top' }">
      <div
        class="flex items-center gap-1 text-sm"
        :class="{ 'text-error': isError }"
      >
        <UiTextView variant="caption">
          {{ $t(`character.characteristic.${fieldGroupKey}.children.${fieldKey}.title`) }}
        </UiTextView>

        <UIcon
          v-if="isError"
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
                  {{ $n(characteristicBonusByKey[fieldKey]!.value, { style: characteristicBonusByKey[fieldKey]!.style, minimumFractionDigits: 0 }) }}
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
            :disabled="inputProps.modelValue <= inputProps.min"
            :data-aq-reset-field="`${fieldGroupKey}:${fieldKey}`"
            @click="$emit('resetField')"
          />
        </UTooltip>
        <UInputNumber
          :data-aq-control="`${fieldGroupKey}:${fieldKey}`"
          variant="subtle"
          :color="isError ? 'error' : 'neutral'"
          :ui="{
            base: 'w-22',
            decrement: 'flex items-center gap-0.5',
          }"
          :model-value="inputProps.modelValue"
          @update:model-value="$emit('update:modelValue', $event)"
        >
          <template #decrement>
            <UButton
              variant="link"
              color="neutral"
              icon="i-lucide-minus"
              :disabled="inputProps.modelValue <= inputProps.min"
            />
          </template>
          <template #increment>
            <UButton
              variant="link"
              color="neutral"
              icon="i-lucide-plus"
              :disabled="inputProps.max <= inputProps.modelValue"
            />
          </template>
        </UInputNumber>
        <UTooltip :text="$t('character.characteristic.fillField')">
          <UButton
            variant="subtle"
            color="neutral"
            icon="i-lucide-chevrons-right"
            :disabled="inputProps.max <= inputProps.modelValue"
            :data-aq-fill-field="`${fieldGroupKey}:${fieldKey}`"
            @click="$emit('fillField')"
          />
        </UTooltip>
      </UFieldGroup>
    </template>
  </UiDataCell>
</template>
