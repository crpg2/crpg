<script setup lang="ts">
import {
  maxExperienceMultiplierForGeneration,
  maximumLevel,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'

import { getExperienceForLevel } from '~/services/character-service'

const props = defineProps<{
  character: Character
  userExperienceMultiplier: number
}>()

const animatedCharacterExperience = useTransition(() => props.character.experience)
const currentLevelExperience = computed(() => getExperienceForLevel(props.character.level))
const nextLevelExperience = computed(() => getExperienceForLevel(props.character.level + 1))
const experiencePercentToNextLevel = computed(() =>
  percentOf(
    props.character.experience - currentLevelExperience.value,
    nextLevelExperience.value - currentLevelExperience.value,
  ),
)
</script>

<template>
  <div class="grid grid-cols-2 gap-2">
    <UiSimpleTableRow
      :label="$t('character.statistics.level.title')"
    >
      <UiDataCell :class="{ 'text-warning': character.forTournament }">
        {{ character.level }}
        <template v-if="character.forTournament" #rightContent>
          <UIcon name="crpg:lock" class="size-4" />
        </template>
      </UiDataCell>

      <template #tooltip-content>
        <UiTooltipContent
          :title="character.forTournament
            ? $t('character.statistics.level.lockedTooltip.title')
            : $t('character.statistics.level.tooltip.title')"
        >
          <template v-if="!character.forTournament" #description>
            <p class="text-primary">
              {{ $t('character.statistics.level.tooltip.maximum', { maxLevel: maximumLevel }) }}
            </p>
          </template>
        </UiTooltipContent>
      </template>
    </UiSimpleTableRow>

    <template v-if="!character.forTournament">
      <UiSimpleTableRow
        :label="$t('character.statistics.generation.title')"
        :value="String(character.generation)"
        :tooltip="{ title: $t('character.statistics.generation.tooltip.title') }"
      />

      <UiSimpleTableRow
        :label="$t('character.statistics.expMultiplier.title')"
        :value="$t('character.format.expMultiplier', { multiplier: $n(userExperienceMultiplier) })"
      >
        <template #tooltip-content>
          <UiTooltipContent
            :title="$t('character.statistics.expMultiplier.tooltip.title')"
          >
            <template #description>
              <p class="text-primary">
                {{ $t('character.statistics.expMultiplier.tooltip.maximum', { maxExpMulti: $t('character.format.expMultiplier', { multiplier: $n(maxExperienceMultiplierForGeneration) }) }) }}
              </p>
              <p>{{ $t('character.statistics.expMultiplier.tooltip.desc') }}</p>
            </template>
          </UiTooltipContent>
        </template>
      </UiSimpleTableRow>

      <div class="col-span-2 mt-24 px-4">
        <UTooltip>
          <USlider
            :default-value="character.experience"
            :min="currentLevelExperience"
            :max="nextLevelExperience"
            disabled
            size="lg"
            :tooltip="{
              disableClosingTrigger: true,
              open: true,
              arrow: true,
              ui: {
                content: 'text-highlighted font-bold',
              },
              portal: false,
              text: $t('character.statistics.experience.format', {
                exp: $n(Number(animatedCharacterExperience.toFixed(0))),
                expPercent: $n(experiencePercentToNextLevel / 100, 'percent'),
              }),
              content: {
                side: 'top',
              },
            }"
          />
          <template #content>
            <UiTooltipContent :title="$t('character.statistics.experience.tooltip.title')">
              <template #description>
                <i18n-t
                  scope="global"
                  keypath="character.statistics.experience.tooltip.desc"
                  tag="p"
                >
                  <template #remainExpToUp>
                    <strong>{{ $n(nextLevelExperience - character.experience) }}</strong>
                  </template>
                </i18n-t>
              </template>
            </UiTooltipContent>
          </template>
        </UTooltip>

        <div class="mt-1.5 flex justify-between">
          <div class="text-xs text-toned">
            {{ $n(currentLevelExperience) }}
          </div>
          <div class="text-xs text-toned">
            {{ $n(nextLevelExperience) }}
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
