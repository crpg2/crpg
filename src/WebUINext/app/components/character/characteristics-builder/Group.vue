<script setup lang="ts">
import type { CharacteristicKey, CharacteristicSectionKey } from '~/models/character'

import { characteristicBonusByKey } from '~/services/character-service'

defineProps<{
  id: CharacteristicSectionKey
  points: number
  fields: { key: CharacteristicKey }[]
}>()
</script>

<template>
  <UCard :ui="{ body: '!p-0', header: '!px-4' }">
    <template #header>
      <UiDataCell :data-aq-fields-group="id" class="w-full">
        {{ $t(`character.characteristic.${id}.title`) }} - <span class="font-bold" :class="[points < 0 ? 'text-status-danger' : 'text-status-success']">{{ points }}</span>
        <template #rightContent>
          <slot name="title-trailing" />
        </template>
      </UiDataCell>
    </template>

    <UiDataCell
      v-for="field in fields"
      :key="field.key"
      class="w-full px-4 py-2.5 hover:bg-base-200"
    >
      <!-- :class="{
            'text-status-danger': id === 'skills' && !currentSkillRequirementsSatisfied(field as SkillKey),
          }" -->
      <UTooltip
        :ui="{ content: 'max-w-72' }"
        :content="{
          side: 'top',
        }"
      >
        <div class="flex items-center gap-1 text-xs">
          {{ $t(`character.characteristic.${id}.children.${field.key}.title`) }}
          <!-- <OIcon
            v-if="id === 'skills' && !currentSkillRequirementsSatisfied(field as SkillKey)"
            icon="alert-circle"
            size="xs"
          /> -->
        </div>
        <template #content>
          <div class="prose prose-invert">
            <h4>
              {{ $t(`character.characteristic.${id}.children.${field.key}.title`) }}
            </h4>

            <i18n-t
              scope="global"
              :keypath="`character.characteristic.${id}.children.${field.key}.desc`"
              tag="p"
            >
              <template
                v-if="field.key in characteristicBonusByKey"
                #value
              >
                <span class="font-bold text-content-100">
                  {{
                    $n(characteristicBonusByKey[field.key]!.value, {
                      style: characteristicBonusByKey[field.key]!.style,
                      minimumFractionDigits: 0,
                    })
                  }}
                </span>
              </template>
            </i18n-t>

            <p
              v-if="$t(`character.characteristic.${id}.children.${field.key}.requires`)"
              class="text-status-warning"
            >
              {{ $t('character.characteristic.requires.title') }}:
              {{ $t(`character.characteristic.${id}.children.${field.key}.requires`) }}
            </p>
          </div>
        </template>
      </UTooltip>

      <template #rightContent>
        <slot v-bind="{ field }" />
      </template>
    </UiDataCell>
  </UCard>
</template>
