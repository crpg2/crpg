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
      :tooltip="character.forTournament
        ? { title: $t('character.statistics.level.lockedTooltip.title', { maxLevel: maximumLevel }) }
        : { title: $t('character.statistics.level.tooltip.title', { maxLevel: maximumLevel }) }
      "
    >
      <UiDataCell :class="{ 'text-warning': character.forTournament }">
        {{ character.level }}
        <template v-if="character.forTournament" #rightContent>
          <UIcon name="crpg:lock" class="size-4" />
        </template>
      </UiDataCell>
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
        :tooltip="{
          title: $t('character.statistics.expMultiplier.tooltip.title', { maxExpMulti: $t('character.format.expMultiplier', { multiplier: $n(maxExperienceMultiplierForGeneration) }) }),
          description: $t('character.statistics.expMultiplier.tooltip.desc'),
        }"
      />

      <div class="col-span-2 mt-16 px-4">
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
            <div
              class="prose prose-invert"
              v-html="$t('character.statistics.experience.tooltip', { remainExpToUp: $n(nextLevelExperience - character.experience) })"
            />
          </template>
        </UTooltip>

        <div class="mt-1.5 flex justify-between">
          <div class="text-2xs text-muted">
            {{ $n(currentLevelExperience) }}
          </div>
          <div class="text-2xs text-muted">
            {{ $n(nextLevelExperience) }}
          </div>
        </div>
      </div>
    </template>
  </div>
</template>
