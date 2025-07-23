<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import { UInputNumber } from '#components'
import { maximumLevel, minimumLevel } from '~root/data/constants.json'
import { defu } from 'defu'

import type {
  CharacterAttributes,
  CharacterCharacteristics,
  CharacteristicConversion,
  CharacterSkills,
  CharacterWeaponProficiencies,
} from '~/models/character'

import { useCharacterCharacteristicBuilder } from '~/composables/character/use-character-characteristic'
import { CHARACTERISTIC_CONVERSION } from '~/models/character'
import {
  attributePointsForLevel,
  computeHealthPoints,
  createCharacteristics,
  createDefaultCharacteristic,
  getExperienceForLevel,
  skillPointsForLevel,
  wppForLevel,
} from '~/services/character-service'

const { t } = useI18n()

const route = useRoute('builder')
const router = useRouter()

const initialCharacteristics = ref<CharacterCharacteristics>(
  createCharacteristics(
    defu(
      {
        ...(route.query?.attributes && { attributes: route.query.attributes as Partial<CharacterAttributes> }),
        ...(route.query?.skills && { skills: route.query.skills as Partial<CharacterSkills> }),
        ...(route.query?.weaponProficiencies && { weaponProficiencies: route.query.weaponProficiencies as Partial<CharacterWeaponProficiencies> }),
      },
      createDefaultCharacteristic(),
    ),
  ),
)

const {
  characteristics,
  canConvertAttributesToSkills,
  canConvertSkillsToAttributes,
  currentSkillRequirementsSatisfied,
  convertAttributeToSkills,
  convertSkillsToAttribute,
  getInputProps,
  onInput,
  reset: resetCharacterBuilderState,
} = useCharacterCharacteristicBuilder(initialCharacteristics)

const level = useRouteQuery('level', minimumLevel, { mode: 'replace' })

const convertRateAttributesToSkills = useRouteQuery('convertRateAttributesToSkills', 0, { mode: 'replace' })
const convertRateSkillsToAttributes = useRouteQuery('convertRateSkillsToAttributes', 0, { mode: 'replace' })

watch(level, () => {
  resetCharacterBuilderState()
  convertRateAttributesToSkills.value = 0
  convertRateSkillsToAttributes.value = 0

  initialCharacteristics.value = createCharacteristics(defu({
    attributes: {
      points: attributePointsForLevel(level.value),
    },
    skills: {
      points: skillPointsForLevel(level.value),
    },
    weaponProficiencies: {
      points: wppForLevel(level.value),
    },
  }, createDefaultCharacteristic()))
})

watchDebounced(
  characteristics,
  () => {
    // @ts-expect-error TODO:
    router.replace({ query: { ...route.query, ...characteristics.value } })
  },
  { debounce: 500 },
)

const experienceForLevel = computed(() => getExperienceForLevel(level.value))
const experienceForNextLevel = computed(() => getExperienceForLevel(level.value + 1))

const weight = useRouteQuery('weight', 0, { mode: 'replace' })
const weaponLength = useRouteQuery('weaponLength', 0, { mode: 'replace' })

// TODO: unit
const convertCharacteristics = (conversion: CharacteristicConversion) => {
  if (conversion === CHARACTERISTIC_CONVERSION.AttributesToSkills) {
    if (convertRateSkillsToAttributes.value > 0) {
      convertRateSkillsToAttributes.value -= 1
    }
    else {
      convertRateAttributesToSkills.value += 1
    }

    convertAttributeToSkills()
    return
  }

  if (convertRateAttributesToSkills.value > 0) {
    convertRateAttributesToSkills.value -= 1
  }
  else {
    convertRateSkillsToAttributes.value += 1
  }

  convertSkillsToAttribute()
}

const healthPoints = computed(() =>
  computeHealthPoints(
    characteristics.value.skills.ironFlesh,
    characteristics.value.attributes.strength,
  ),
)

const onReset = async () => {
  initialCharacteristics.value = createDefaultCharacteristic()
  resetCharacterBuilderState()
  router.replace({ query: {} })
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
      <div
        class="mx-auto max-w-4xl"
      >
        <h1 class="mb-14 text-center text-xl">
          {{ $t('builder.title') }}
        </h1>

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
                <span class="font-bold text-primary">{{ level }}</span>
              </template>

              <template #exp>
                <span class="font-bold">{{ $n(experienceForLevel) }}</span>
              </template>

              <template #nextExp>
                <span class="font-bold">{{ $n(experienceForNextLevel) }}</span>
              </template>

              <template #nextLevel>
                <span class="font-bold text-primary">{{ level + 1 }}</span>
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
                <span class="font-bold text-primary">{{ level }}</span>
              </template>

              <template #exp>
                <span class="font-bold">{{ $n(experienceForLevel) }}</span>
              </template>
            </i18n-t>

            <div>
              <USlider
                v-model="level"
                :min="minimumLevel"
                :max="maximumLevel"
              />

              <div class="mt-2 flex justify-between">
                <div class="text-2xs text-muted">
                  {{ $n(minimumLevel) }}
                </div>
                <div class="text-center text-2xs text-muted">
                  {{ $t('builder.levelChangeAttention') }}
                </div>
                <div class="text-2xs text-muted">
                  {{ $n(maximumLevel) }}
                </div>
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
              :weight="weight"
              :longest-weapon-length="weaponLength"
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
                    size="xs"
                    class="max-w-24"
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
                    size="xs"
                    class="max-w-24"
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
        color="secondary"
        size="lg"
        icon="crpg:reset"
        :label="$t('action.reset')"
        @click="onReset"
      />
      <UButton
        size="lg"
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
