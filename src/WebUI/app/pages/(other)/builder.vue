<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import { UInputNumber } from '#components'
import { maximumLevel, minimumLevel } from '~root/data/constants.json'

import type {
  CharacterAttributes,
  CharacterCharacteristics,
  CharacteristicConversion,
  CharacterSkills,
  CharacterWeaponProficiencies,
} from '~/models/character'

import { useCharacterCharacteristicBuilder } from '~/composables/character/use-character-characteristic-builder'
import { CHARACTERISTIC_CONVERSION } from '~/models/character'
import {
  ATTRIBUTES_TO_SKILLS_RATE,
  createDefaultCharacteristic,
  getCharacterOverallItemsStats,
  getExperienceForLevel,
  SKILLS_TO_ATTRIBUTES_RATE,
} from '~/services/character-service'

const { t } = useI18n()

const route = useRoute('builder')
const router = useRouter()

const level = useRouteQuery('level', minimumLevel)
const localLevel = ref(level.value)
function onChangeLevel() {
  level.value = localLevel.value
}

const initialCharacteristics = ref<CharacterCharacteristics>(
  (route.query?.attributes && route.query?.skills && route.query?.weaponProficiencies)
    ? {
      attributes: route.query.attributes as unknown as CharacterAttributes,
      skills: route.query.skills as unknown as CharacterSkills,
      weaponProficiencies: route.query.weaponProficiencies as unknown as CharacterWeaponProficiencies,
    } satisfies CharacterCharacteristics
    : createDefaultCharacteristic(),
)

const {
  characteristics,
  canConvertAttributesToSkills,
  canConvertSkillsToAttributes,
  currentSkillRequirementsSatisfied,
  getInputProps,
  onInput,
  reset: resetCharacterBuilderState,
  healthPoints,
} = useCharacterCharacteristicBuilder(initialCharacteristics)

const convertRateAttributesToSkills = useRouteQuery('convertRateAttributesToSkills', 0)
const convertRateSkillsToAttributes = useRouteQuery('convertRateSkillsToAttributes', 0)

const onReset = () => {
  localLevel.value = minimumLevel
  initialCharacteristics.value = createDefaultCharacteristic()
  resetCharacterBuilderState()
  router.replace({ query: {} })
}

watch(level, () => {
  resetCharacterBuilderState()
  convertRateAttributesToSkills.value = 0
  convertRateSkillsToAttributes.value = 0
  initialCharacteristics.value = createDefaultCharacteristic(level.value)
})

watchDebounced(
  characteristics,
  () => {
    // @ts-expect-error ///
    router.replace({ query: { ...route.query, ...characteristics.value } })
  },
  { debounce: 500 },
)

const experienceForLevel = computed(() => getExperienceForLevel(localLevel.value))
const experienceForNextLevel = computed(() => getExperienceForLevel(localLevel.value + 1))

const weight = useRouteQuery('weight', 0)
const weaponLength = useRouteQuery('weaponLength', 0)
const mountSpeed = useRouteQuery('mountSpeed', 0)
const mountHarnessWeight = useRouteQuery('mountHarnessWeight', 0)

// TODO: unit
const convertCharacteristics = (conversion: CharacteristicConversion) => {
  if (conversion === CHARACTERISTIC_CONVERSION.AttributesToSkills) {
    if (convertRateSkillsToAttributes.value > 0) {
      convertRateSkillsToAttributes.value -= 1
    }
    else {
      convertRateAttributesToSkills.value += 1
    }

    // convertAttributeToSkills
    initialCharacteristics.value.attributes.points -= ATTRIBUTES_TO_SKILLS_RATE
    initialCharacteristics.value.skills.points += SKILLS_TO_ATTRIBUTES_RATE
    return
  }

  if (convertRateAttributesToSkills.value > 0) {
    convertRateAttributesToSkills.value -= 1
  }
  else {
    convertRateSkillsToAttributes.value += 1
  }

  // convertSkillsToAttribute
  initialCharacteristics.value.skills.points -= SKILLS_TO_ATTRIBUTES_RATE
  initialCharacteristics.value.attributes.points += ATTRIBUTES_TO_SKILLS_RATE
}

const { copy } = useClipboard()
const toast = useToast()

const onShare = () => {
  copy(window.location.href)
  toast.add({
    title: t('builder.share.notify.success'),
    color: 'success',
    close: false,
  })
}
</script>

<template>
  <div
    class="
      py-8
      md:py-16
    "
  >
    <UContainer>
      <div class="mx-auto max-w-4xl">
        <UiHeading :title="$t('builder.title')" class="mb-6" />

        <div class="relative space-y-8">
          <div class="space-y-5">
            <i18n-t
              v-if="level < maximumLevel"
              scope="global"
              keypath="builder.levelIntervalTpl"
              tag="div"
              class="text-center"
            >
              <template #level>
                <span class="font-bold text-primary">{{ localLevel }}</span>
              </template>

              <template #exp>
                <span class="font-bold">{{ $n(experienceForLevel) }}</span>
              </template>

              <template #nextExp>
                <span class="font-bold">{{ $n(experienceForNextLevel) }}</span>
              </template>

              <template #nextLevel>
                <span class="font-bold text-primary">{{ localLevel + 1 }}</span>
              </template>
            </i18n-t>

            <i18n-t
              v-else
              scope="global"
              keypath="builder.levelTpl"
              tag="div"
              class="text-center"
            >
              <template #level>
                <span class="font-bold text-primary">{{ localLevel }}</span>
              </template>

              <template #exp>
                <span class="font-bold">{{ $n(experienceForLevel) }}</span>
              </template>
            </i18n-t>

            <div>
              <USlider
                v-model="localLevel"
                :min="minimumLevel"
                :max="maximumLevel"
                @change="onChangeLevel"
              />

              <div class="mt-2 flex justify-between">
                <UiTextView variant="caption-sm">
                  {{ $n(minimumLevel) }}
                </UiTextView>
                <UiTextView variant="caption-sm">
                  {{ $t('builder.levelChangeAttention') }}
                </UiTextView>
                <UiTextView variant="caption-sm">
                  {{ $n(maximumLevel) }}
                </UiTextView>
              </div>
            </div>
          </div>

          <div class="statsGrid grid items-start gap-6">
            <CharacterCharacteristicsBuilder
              :get-input-props="(group, field) => getInputProps(group, field, true) "
              :characteristics
              :check-current-skill-requirements-satisfied="currentSkillRequirementsSatisfied"
              :convert-attributes-to-skills-state="{ disabled: !canConvertAttributesToSkills, count: convertRateAttributesToSkills }"
              :convert-skills-to-attributes-state="{ disabled: !canConvertSkillsToAttributes, count: convertRateSkillsToAttributes }"
              @input="onInput"
              @convert-attributes-to-skills="convertCharacteristics(CHARACTERISTIC_CONVERSION.AttributesToSkills)"
              @convert-skills-to-attributes="convertCharacteristics(CHARACTERISTIC_CONVERSION.SkillsToAttributes)"
            />

            <CharacterStats
              style="grid-area: stats"
              :characteristics="characteristics"
              :items-overall-stats="{
                ...getCharacterOverallItemsStats(),
                weight,
                longestWeaponLength: weaponLength,
                mountSpeedBase: mountSpeed,
                mountHarnessWeight,
              }"
              :hidden-rows="['weight']"
              :health-points="healthPoints"
            >
              <template #leading>
                <UiSimpleTableRow
                  :label="$t('character.stats.weight.title')"
                  :tooltip="{ title: $t('builder.weight') }"
                >
                  <UInputNumber
                    v-model="weight"
                    class="max-w-24"
                    color="neutral"
                    :min="0"
                  />
                </UiSimpleTableRow>

                <UiSimpleTableRow
                  :label="$t('builder.weaponLength.title')"
                  :tooltip="{
                    title: $t('builder.weaponLength.title'),
                    description: $t('builder.weaponLength.desc'),
                  }"
                >
                  <UInputNumber
                    v-model="weaponLength"
                    class="max-w-24"
                    color="neutral"
                    :min="0"
                  />
                </UiSimpleTableRow>

                <UiSimpleTableRow
                  :label="$t('builder.mountSpeed.title')"
                  :tooltip="{ title: $t('builder.mountSpeed.title') }"
                >
                  <UInputNumber
                    v-model="mountSpeed"
                    class="max-w-24"
                    color="neutral"
                    :min="0"
                  />
                </UiSimpleTableRow>

                <UiSimpleTableRow
                  :label="$t('builder.mountHarnessWeight.title')"
                  :tooltip="{ title: $t('builder.mountHarnessWeight.title') }"
                >
                  <UInputNumber
                    v-model="mountHarnessWeight"
                    class="max-w-24"
                    :min="0"
                    color="neutral"
                  />
                </UiSimpleTableRow>
              </template>
            </CharacterStats>
          </div>
        </div>
      </div>
    </UContainer>

    <div
      class="
        sticky bottom-0 left-0 flex w-full items-center justify-center gap-2 py-4 backdrop-blur-sm
      "
    >
      <UButton
        variant="outline"
        color="neutral"
        size="xl"
        icon="crpg:reset"
        :label="$t('action.reset')"
        @click="onReset"
      />
      <UButton
        size="xl"
        icon="crpg:share"
        :label="$t('action.share')"
        @click="onShare"
      />
    </div>
  </div>
</template>

<style lang="css">
.statsGrid {
  grid-template-areas:
    'attributes skills stats'
    'weaponProficiencies skills stats';
  grid-template-columns: 1fr 1fr 1fr;
  grid-template-rows: auto auto auto;
}
</style>
