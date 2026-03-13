<script setup lang="ts">
import { UTooltip } from '#components'

import type { CharacteristicKey, CharacteristicSectionKey } from '~/models/character'
import type { CharacteristicProps } from '~/services/character-service'

import { characteristicBonusByKey } from '~/services/character-service'

const { inputProps } = defineProps<{
  section: CharacteristicSectionKey
  characteristic: CharacteristicKey
  inputProps: CharacteristicProps
}>()

defineEmits<{
  'fillField': []
  'resetField': []
  'update:modelValue': [value: number]
}>()

const isError = computed(() => inputProps.requirement?.satisfied === false)
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
            <UiTextView
              variant="p"
              class="text-warning"
            >
              {{ $t('character.characteristic.requires.text', {
                text: [
                  ...(inputProps.requirement ? [
                    $t('character.characteristic.requires.format', {
                      points: inputProps.requirement?.points,
                      characteristic: $t(`character.characteristic.${inputProps.requirement.section}.children.${inputProps.requirement.characteristic}.title`).toLowerCase(),
                    }),
                  ] : []),
                  $t('character.characteristic.requires.format', {
                    points: inputProps.costToIncrease,
                    characteristic: $t(`character.characteristic.${section}.title`).toLowerCase(),
                  }),
                ].join(', '),
              }) }}
            </UiTextView>
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
            :disabled="inputProps.value <= inputProps.min"
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
          :model-value="inputProps.value"
          @update:model-value="$emit('update:modelValue', $event)"
        >
          <template #decrement>
            <UButton
              variant="link"
              color="neutral"
              icon="i-lucide-minus"
              :disabled="inputProps.value <= inputProps.min"
            />
          </template>
          <template #increment>
            <UButton
              variant="link"
              color="neutral"
              icon="i-lucide-plus"
              :disabled="inputProps.max <= inputProps.value"
            />
          </template>
        </UInputNumber>
        <UTooltip :text="$t('character.characteristic.fillField')">
          <UButton
            variant="subtle"
            color="neutral"
            icon="i-lucide-chevrons-right"
            :disabled="inputProps.max <= inputProps.value"
            :data-aq-fill-field="`${section}:${characteristic}`"
            @click="$emit('fillField')"
          />
        </UTooltip>
      </UFieldGroup>
    </template>
  </UiDataCell>
</template>
