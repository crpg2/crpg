<script setup lang="ts">
import type { SkillKey } from '~/models/character'

import { useCharacterCharacteristic } from '~/composables/character/use-character-characteristic'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { CharacteristicConversion } from '~/models/character'
import {
  characteristicBonusByKey,
  computeHealthPoints,
  convertCharacterCharacteristics,
  updateCharacterCharacteristics,
} from '~/services/characters-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'
import {
  characterCharacteristicsKey,
  characterItemsStatsKey,
  characterKey,
} from '~/symbols/character'
import { sleep } from '~/utils/promise'

definePage({
  meta: {
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const userStore = useUserStore()

const character = injectStrict(characterKey)
const { characterCharacteristics, setCharacterCharacteristics } = injectStrict(characterCharacteristicsKey)
const itemsStats = injectStrict(characterItemsStatsKey)

const {
  characteristics,
  //
  canConvertAttributesToSkills,
  canConvertSkillsToAttributes,
  currentSkillRequirementsSatisfied,
  isChangeValid,
  wasChangeMade,
  //
  formSchema,
  //
  getInputProps,
  onInput,
  reset,
} = useCharacterCharacteristic(characterCharacteristics)

const healthPoints = computed(() =>
  computeHealthPoints(
    characteristics.value.skills.ironFlesh,
    characteristics.value.attributes.strength,
  ),
)

const { execute: onCommitCharacterCharacteristics, loading: commitingCharacterCharacteristics } = useAsyncCallback(async () => {
  setCharacterCharacteristics(
    await updateCharacterCharacteristics(character.value.id, characteristics.value),
  )
  reset()
  await userStore.fetchCharacters()
  notify(t('character.characteristic.commit.notify'))
})

const { execute: onConvertCharacterCharacteristics, loading: convertingCharacterCharacteristics } = useAsyncCallback(async (conversion: CharacteristicConversion) => {
  await Promise.all([
    setCharacterCharacteristics(
      await convertCharacterCharacteristics(character.value.id, conversion),
    ),
    sleep(400),
  ])
})

const { loadCharacterLimitations, respecCapability, respecializingCharacter, onRespecializeCharacter } = useCharacterRespec()

const fetchPageData = (characterId: number) =>
  Promise.all([
    loadCharacterLimitations(0, { id: characterId }),
  ])

fetchPageData(character.value.id)

onBeforeRouteUpdate(async (to, from) => {
  if (to.name === from.name) {
    // if character changed
    // @ts-expect-error TODO:
    const characterId = Number(to.params.id)
    await fetchPageData(characterId)
  }
  return true
})

onBeforeRouteUpdate(() => {
  reset()
  return true
})
</script>

<template>
  <div class="relative mx-auto max-w-4xl">
    <OLoading
      :active="convertingCharacterCharacteristics || commitingCharacterCharacteristics || respecializingCharacter"
      icon-size="xl"
    />
    <div class="statsGrid mb-8 grid gap-6">
      <div
        v-for="fieldsGroup in formSchema"
        :key="fieldsGroup.key"
        class="space-y-3"
        :style="{ 'grid-area': fieldsGroup.key }"
      >
        <div
          class="flex items-center justify-between gap-4"
          :data-aq-fields-group="fieldsGroup.key"
        >
          <div>
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
              rounded
              outlined
              :disabled="!canConvertAttributesToSkills"
              icon-right="convert"
              data-aq-convert-attributes-action
              @click="onConvertCharacterCharacteristics(CharacteristicConversion.AttributesToSkills) "
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
                    <!-- TODO: 1, 2 to constants.json -->
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
              icon-right="convert"
              data-aq-convert-skills-action
              @click="onConvertCharacterCharacteristics(CharacteristicConversion.SkillsToAttributes)"
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
            class="flex items-center justify-between gap-2 px-4 py-2.5 text-2xs hover:bg-base-200"
          >
            <VTooltip>
              <div
                class="flex items-center gap-1 text-2xs"
                :class="{
                  'text-status-danger':
                    fieldsGroup.key === 'skills'
                    && !currentSkillRequirementsSatisfied(field.key as SkillKey),
                }"
              >
                {{ $t(`character.characteristic.${fieldsGroup.key}.children.${field.key}.title`) }}

                <OIcon
                  v-if="
                    fieldsGroup.key === 'skills'
                      && !currentSkillRequirementsSatisfied(field.key as SkillKey)
                  "
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
              :exponential="0.5"
              readonly
              :data-aq-control="`${fieldsGroup.key}:${field.key}`"
              v-bind="getInputProps(fieldsGroup.key, field.key)"
              @update:model-value="onInput(fieldsGroup.key, field.key, $event)"
            />
          </div>
        </div>
      </div>

      <div
        class="grid gap-2 self-start rounded-xl border border-border-200 py-2 text-2xs"
        style="grid-area: stats"
      >
        <CharacterStats
          :characteristics="characteristics!"
          :weight="itemsStats.weight"
          :longest-weapon-length="itemsStats.longestWeaponLength"
          :health-points="healthPoints"
        />
      </div>
    </div>

    <div
      class="sticky bottom-0 left-0 w-full py-4 backdrop-blur-sm"
    >
      <div class="flex max-w-4xl items-center justify-center gap-4">
        <OButton
          :disabled="!wasChangeMade"
          variant="secondary"
          size="lg"
          icon-left="reset"
          :label="$t('action.reset')"
          data-aq-reset-action
          @click="reset"
        />

        <ConfirmActionTooltip @confirm="onCommitCharacterCharacteristics">
          <OButton
            variant="primary"
            size="lg"
            icon-left="check"
            :disabled="!wasChangeMade || !isChangeValid"
            :label="$t('action.commit')"
            data-aq-commit-action
          />
        </ConfirmActionTooltip>

        <CharacterRespecButtonModal
          :respec-capability
          :character
          @respec="() => onRespecializeCharacter(character.id)"
        />
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
