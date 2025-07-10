<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import { maximumLevel, minimumLevel } from '~root/data/constants.json'
import { defu } from 'defu'

import type {
  CharacterAttributes,
  CharacterCharacteristics,
  CharacterSkills,
  CharacterWeaponProficiencies,
  SkillKey,
} from '~/models/character'

import { useCharacterCharacteristic } from '~/composables/character/use-character-characteristic'
import { CharacteristicConversion } from '~/models/character'
import {
  attributePointsForLevel,
  characteristicBonusByKey,
  computeHealthPoints,
  createCharacteristics,
  createDefaultCharacteristic,
  getExperienceForLevel,
  skillPointsForLevel,
  wppForLevel,
} from '~/services/characters-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const route = useRoute()
const router = useRouter()

const initialCharacteristics = ref<CharacterCharacteristics>(
  createCharacteristics(
    defu(
      {
        ...(route.query?.attributes && {
          attributes: route.query.attributes as Partial<CharacterAttributes>,
        }),
        ...(route.query?.skills && { skills: route.query.skills as Partial<CharacterSkills> }),
        ...(route.query?.weaponProficiencies && {
          weaponProficiencies: route.query
            .weaponProficiencies as Partial<CharacterWeaponProficiencies>,
        }),
      },
      createDefaultCharacteristic(),
    ),
  ),
)

const level = computed({
  get() {
    return Number(route.query.level) || minimumLevel
  },

  async set(value: number) {
    reset()
    await nextTick()

    await router.push({ query: { level: value } })

    initialCharacteristics.value = {
      attributes: {
        ...initialCharacteristics.value.attributes,
        points: attributePointsForLevel(value),
      },
      skills: {
        ...initialCharacteristics.value.skills,
        points: skillPointsForLevel(value),
      },
      weaponProficiencies: {
        ...initialCharacteristics.value.weaponProficiencies,
        points: wppForLevel(value),
      },
    }
  },
})

const experienceForLevel = computed(() => getExperienceForLevel(level.value))
const experienceForNextLevel = computed(() => getExperienceForLevel(level.value + 1))

const weight = computed({
  get() {
    return Number(route.query.weight) || 0
  },

  set(value: number) {
    router.push({ query: { ...route.query, weight: value } })
  },
})

const weaponLength = computed({
  get() {
    return Number(route.query.weaponLength) || 0
  },

  set(value: number) {
    router.push({ query: { ...route.query, weaponLength: value } })
  },
})

const {
  characteristics,
  //
  canConvertAttributesToSkills,
  canConvertSkillsToAttributes,
  currentSkillRequirementsSatisfied,
  //
  formSchema,
  //
  convertAttributeToSkills,
  convertSkillsToAttribute,
  getInputProps,
  onInput,
  reset,
} = useCharacterCharacteristic(initialCharacteristics)

watch(characteristics, (val) => {
  router.push({
    // @ts-expect-error TODO:
    query: { ...route.query, ...val },
  })
})

const healthPoints = computed(() =>
  computeHealthPoints(
    characteristics.value.skills.ironFlesh,
    characteristics.value.attributes.strength,
  ),
)

const convertRate = computed({
  get() {
    return defu(route.query.convert, {
      [CharacteristicConversion.AttributesToSkills]: 0,
      [CharacteristicConversion.SkillsToAttributes]: 0,
    })
  },
  set(_value: Record<CharacteristicConversion, number>) {},
})

const convertCharacteristics = async (conversion: CharacteristicConversion) => {
  if (conversion === CharacteristicConversion.AttributesToSkills) {
    // TODO: unit
    if (convertRate.value.SkillsToAttributes > 0) {
      convertRate.value = {
        AttributesToSkills: convertRate.value.AttributesToSkills,
        SkillsToAttributes: (convertRate.value.SkillsToAttributes -= 1),
      }
    }
    else {
      convertRate.value = {
        AttributesToSkills: (convertRate.value.AttributesToSkills += 1),
        SkillsToAttributes: convertRate.value.SkillsToAttributes,
      }
    }

    // @ts-expect-error TODO:
    await router.push({ query: { ...route.query, convert: convertRate.value } })
    convertAttributeToSkills()
    return
  }

  // TODO: unit
  if (convertRate.value.AttributesToSkills > 0) {
    convertRate.value = {
      AttributesToSkills: (convertRate.value.AttributesToSkills -= 1),
      SkillsToAttributes: convertRate.value.SkillsToAttributes,
    }
  }
  else {
    convertRate.value = {
      AttributesToSkills: convertRate.value.AttributesToSkills,
      SkillsToAttributes: (convertRate.value.SkillsToAttributes += 1),
    }
  }

  // @ts-expect-error TODO:
  await router.push({ query: { ...route.query, convert: convertRate.value } })
  convertSkillsToAttribute()
}

const onReset = async () => {
  initialCharacteristics.value = createDefaultCharacteristic()
  reset()
  await nextTick()
  router.push({ query: {} })
}

const { copy } = useClipboard()

const onShare = () => {
  copy(window.location.href)
  notify(t('builder.share.notify.success'))
}
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-4xl py-8 md:py-16">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('builder.title') }}
      </h1>

      <div class="relative">
        <div class="mb-12 space-y-5">
          <i18n-t
            v-if="level < maximumLevel"
            scope="global"
            keypath="builder.levelIntervalTpl"
            tag="div"
            class="text-center"
          >
            <template #level>
              <span class="font-bold text-content-100">{{ level }}</span>
            </template>

            <template #exp>
              <span class="font-bold">{{ $n(experienceForLevel) }}</span>
            </template>

            <template #nextExp>
              <span class="font-bold">{{ $n(experienceForNextLevel) }}</span>
            </template>

            <template #nextLevel>
              <span class="font-bold text-content-100">{{ level + 1 }}</span>
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
              <span class="font-bold text-content-100">{{ level }}</span>
            </template>

            <template #exp>
              <span class="font-bold">{{ $n(experienceForLevel) }}</span>
            </template>
          </i18n-t>

          <VueSlider
            v-model="level"
            lazy
            :min="minimumLevel"
            :max="maximumLevel"
            :marks="[minimumLevel, maximumLevel]"
          />

          <div class="text-center text-2xs text-content-400">
            {{ $t('builder.levelChangeAttention') }}
          </div>
        </div>

        <div class="statsGrid mb-8 grid gap-6">
          <div
            v-for="fieldsGroup in formSchema"
            :key="fieldsGroup.key"
            class="space-y-3"
            :style="{ 'grid-area': fieldsGroup.key }"
          >
            <div class="flex items-center justify-between gap-4">
              <div class="">
                {{ $t(`character.characteristic.${fieldsGroup.key}.title`) }} -
                <span
                  class="font-bold"
                  :class="[
                    characteristics[fieldsGroup.key].points < 0
                      ? 'text-status-danger'
                      : 'text-status-success',
                  ]"
                >
                  {{ characteristics[fieldsGroup.key].points }}
                </span>
              </div>

              <VTooltip v-if="fieldsGroup.key === 'attributes'">
                <OButton
                  variant="primary"
                  size="xs"
                  outlined
                  rounded
                  :disabled="!canConvertAttributesToSkills"
                  icon-left="convert"
                  data-aq-convert-attributes-action
                  :label="String(convertRate.AttributesToSkills)"
                  @click="convertCharacteristics(CharacteristicConversion.AttributesToSkills)"
                />
                <template #popper>
                  <div class="prose prose-invert">
                    <h4>
                      {{ $t('character.characteristic.convert.attrsToSkills.title') }}
                    </h4>
                    <i18n-t
                      scope="global"
                      keypath="character.characteristic.convert.attrsToSkills.tooltip"
                      class="text-content-200"
                      tag="p"
                    >
                      <template #attribute>
                        <span class="font-bold text-status-danger">1</span>
                      </template>
                      <template #skill>
                        <span class="font-bold text-status-success">2</span>
                      </template>
                    </i18n-t>
                  </div>
                </template>
              </VTooltip>

              <VTooltip v-else-if="fieldsGroup.key === 'skills'">
                <OButton
                  variant="primary"
                  size="xs"
                  rounded
                  outlined
                  :disabled="!canConvertSkillsToAttributes"
                  icon-left="convert"
                  :label="String(convertRate.SkillsToAttributes)"
                  data-aq-convert-skills-action
                  @click="convertCharacteristics(CharacteristicConversion.SkillsToAttributes)"
                />
                <template #popper>
                  <div class="prose prose-invert">
                    <h4>
                      {{ $t('character.characteristic.convert.skillsToAttrs.title') }}
                    </h4>
                    <i18n-t
                      scope="global"
                      keypath="character.characteristic.convert.skillsToAttrs.tooltip"
                      class="text-content-200"
                      tag="p"
                    >
                      <!-- TODO: 1, 2 to constants.json -->
                      <template #skill>
                        <span class="font-bold text-status-danger">2</span>
                      </template>
                      <template #attribute>
                        <span class="font-bold text-status-success">1</span>
                      </template>
                    </i18n-t>
                  </div>
                </template>
              </VTooltip>
            </div>

            <div class="rounded-xl border border-border-200 py-2">
              <div
                v-for="field in fieldsGroup.children"
                :key="field.key"
                class="flex items-center justify-between px-4 py-2.5 hover:bg-base-200"
              >
                <VTooltip>
                  <div
                    class="flex items-center gap-1 text-2xs"
                    :class="{ 'text-status-danger': fieldsGroup.key === 'skills' && !currentSkillRequirementsSatisfied(field.key as SkillKey) }"
                  >
                    {{
                      $t(`character.characteristic.${fieldsGroup.key}.children.${field.key}.title`)
                    }}

                    <OIcon
                      v-if="fieldsGroup.key === 'skills' && !currentSkillRequirementsSatisfied(field.key as SkillKey)"
                      icon="alert-circle"
                      size="xs"
                    />
                  </div>

                  <template #popper>
                    <div class="prose prose-invert">
                      <h4>
                        {{ $t(`character.characteristic.${fieldsGroup.key}.children.${field.key}.title`) }}
                      </h4>

                      <i18n-t
                        scope="global"
                        :keypath="`character.characteristic.${fieldsGroup.key}.children.${field.key}.desc`"
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
                        v-if="$t(`character.characteristic.${fieldsGroup.key}.children.${field.key}.requires`) !== ''"
                        class="text-status-warning"
                      >
                        {{ $t('character.characteristic.requires.title') }}:
                        {{ $t(`character.characteristic.${fieldsGroup.key}.children.${field.key}.requires`) }}
                      </p>
                    </div>
                  </template>
                </VTooltip>

                <NumericInput
                  v-bind="getInputProps(fieldsGroup.key, field.key, true)"
                  :exponential="0.5"
                  readonly
                  @update:model-value="val => onInput(fieldsGroup.key, field.key, val)"
                />
              </div>
            </div>
          </div>

          <div
            class="grid gap-2 self-start rounded-xl border border-border-200 py-2 text-2xs"
            style="grid-area: stats"
          >
            <SimpleTableRow
              :label="$t('character.stats.weight.title')"
              :tooltip="{ title: $t('builder.weight') }"
            >
              <OInput
                v-model="weight"
                type="number"
                size="xs"
                class="w-16 text-right"
                :min="0"
                :max="100"
              />
            </SimpleTableRow>

            <SimpleTableRow
              :label="$t('builder.weaponLength.title')"
              :tooltip="{
                title: $t('builder.weaponLength.title'),
                description: $t('builder.weaponLength.desc'),
              }"
            >
              <OInput
                v-model="weaponLength"
                type="number"
                size="xs"
                class="w-16 text-right"
                :min="0"
                :max="500"
              />
            </SimpleTableRow>

            <CharacterStats
              :characteristics="characteristics"
              :weight="weight"
              :longest-weapon-length="weaponLength"
              :hidden-rows="['weight']"
              :health-points="healthPoints"
              :mount-speed-base="0"
              :mount-harness-weight="0"
            />
          </div>
        </div>

        <div
          class="sticky bottom-0 left-0 flex w-full items-center justify-center gap-2 bg-bg-main bg-opacity-10 py-4 backdrop-blur-sm"
        >
          <OButton
            variant="secondary"
            outlined
            size="lg"
            icon-left="reset"
            :label="$t('action.reset')"
            @click="onReset"
          />

          <OButton
            variant="primary"
            size="lg"
            icon-left="share"
            :label="$t('action.share')"
            @click="onShare"
          />
        </div>
      </div>
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
