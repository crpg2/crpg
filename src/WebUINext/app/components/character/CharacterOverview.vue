<script setup lang="ts">
import {
  experienceMultiplierByGeneration,
  maxExperienceMultiplierForGeneration,
  maximumLevel,
  minimumRetirementLevel,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'

const { character } = defineProps<{
  character: Character
  userExperienceMultiplier: number
}>()
</script>

<template>
  <div class="grid grid-cols-2 gap-2 text-2xs">
    <UiSimpleTableRow
      :label="$t('character.statistics.level.title')"
      :tooltip="
        character.forTournament
          ? { title: $t('character.statistics.level.lockedTooltip.title', { maxLevel: maximumLevel }) }
          : { title: $t('character.statistics.level.tooltip.title', { maxLevel: maximumLevel }) }
      "
    >
      <div
        class="flex gap-1.5"
        :class="[character.forTournament ? 'text-status-warning' : 'text-content-100']"
      >
        {{ character.level }}
        <OIcon
          v-if="character.forTournament"
          icon="lock"
          size="sm"
        />
      </div>
    </UiSimpleTableRow>

    <template v-if="!character.forTournament">
      <UiSimpleTableRow
        :label="$t('character.statistics.generation.title')"
        :value="String(character.generation)"
        :tooltip="{
          title: $t('character.statistics.generation.tooltip.title'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.statistics.expMultiplier.title')"
        :value=" $t('character.format.expMultiplier', { multiplier: $n(userExperienceMultiplier) })"
        :tooltip="{
          title: $t('character.statistics.expMultiplier.tooltip.title', {
            maxExpMulti: $t('character.format.expMultiplier', {
              multiplier: $n(maxExperienceMultiplierForGeneration),
            }),
          }),
          description: $t('character.statistics.expMultiplier.tooltip.desc'),
        }"
      />

      <div class="col-span-2 mt-12 px-4 py-2.5">
        TODO:
        <!-- <VueSlider
                :key="currentLevelExperience"
                class="!cursor-default !opacity-100"
                :model-value="Number(animatedCharacterExperience.toFixed(0))"
                disabled
                tooltip="always"
                :min="currentLevelExperience"
                :max="nextLevelExperience"
                :marks="[currentLevelExperience, nextLevelExperience]"
              >
                <template #mark="{ pos, value, label }">
                  <div
                    class="absolute top-2.5 whitespace-nowrap"
                    :class="{
                      '-translate-x-full': value === nextLevelExperience,
                    }"
                    :style="{ left: `${pos}%` }"
                  >
                    {{ $n(label) }}
                  </div>
                </template>
                <template #tooltip="{ value }">
                  <div
                    class="vue-slider-dot-tooltip-inner vue-slider-dot-tooltip-inner-top vue-slider-dot-tooltip-inner-disabled"
                  >
                    <div class="flex items-center">
                      <VTooltip placement="bottom">
                        <div class="flex items-center gap-1 font-semibold text-primary">
                          <OIcon
                            icon="experience"
                            size="xl"
                          />
                          {{
                            t('character.statistics.experience.format', {
                              exp: $n(value),
                              expPercent: $n(experiencePercentToNextLEvel / 100, 'percent'),
                            })
                          }}
                        </div>
                        <template #popper>
                          <div
                            class="prose prose-invert"
                            v-html="
                              $t('character.statistics.experience.tooltip', {
                                remainExpToUp: $n(nextLevelExperience - character.experience),
                              })
                            "
                          />
                        </template>
                      </VTooltip>
                    </div>
                  </div>
                </template>
              </VueSlider> -->
      </div>
    </template>
  </div>
</template>
