<script setup lang="ts">
import { timeout } from 'es-toolkit'

import type { CharacterCharacteristics, CharacteristicConversion } from '~/models/character'

import { useCharacter } from '~/composables/character/use-character'
import { useCharacterCharacteristic, useCharacterCharacteristicBuilder } from '~/composables/character/use-character-characteristic'
import { useCharacterItems } from '~/composables/character/use-character-items'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePageLoading } from '~/composables/utils/use-page-loading'
import { CHARACTERISTIC_CONVERSION } from '~/models/character'
import {
  convertCharacterCharacteristics,
  updateCharacterCharacteristics,
} from '~/services/character-service'

const route = useRoute('characters-id-characteristic')
const { t } = useI18n()
const toast = useToast()

const { character } = useCharacter()

const { characterCharacteristics, loadCharacterCharacteristics, healthPoints } = useCharacterCharacteristic()
const { itemsOverallStats } = useCharacterItems()

const setCharacterCharacteristicsSync = (characteristic: CharacterCharacteristics) => {
  characterCharacteristics.value = characteristic
}

const {
  characteristics,
  canConvertAttributesToSkills,
  canConvertSkillsToAttributes,
  currentSkillRequirementsSatisfied,
  isChangeValid,
  wasChangeMade,
  getInputProps,
  onInput,
  reset: resetCharacterCharacteristicBuilder,
} = useCharacterCharacteristicBuilder(characterCharacteristics)

const {
  execute: onConvertCharacterCharacteristics,
  isLoading: convertingCharacterCharacteristics,
} = useAsyncCallback(async (conversion: CharacteristicConversion) => {
  await Promise.all([
    setCharacterCharacteristicsSync(
      await convertCharacterCharacteristics(character.value.id, conversion),
    ),
    timeout(500),
  ])
})

const {
  execute: onCommitCharacterCharacteristics,
  isLoading: commitingCharacterCharacteristics,
} = useAsyncCallback(async () => {
  setCharacterCharacteristicsSync(
    await updateCharacterCharacteristics(character.value.id, characteristics.value),
  )

  resetCharacterCharacteristicBuilder() // TODO:

  toast.add({
    title: t('character.characteristic.commit.notify'),
    close: false,
    color: 'success',
  })
})

const { respecCapability, onRespecializeCharacter: respecializeCharacter } = useCharacterRespec()

const {
  execute: onRespecializeCharacter,
  isLoading: respecializingCharacter,
} = useAsyncCallback(async () => {
  await respecializeCharacter(character.value.id)
  loadCharacterCharacteristics(0, character.value.id)
})

const fetchPageData = (characterId: number) => Promise.all([
  loadCharacterCharacteristics(0, characterId),
])

onBeforeRouteUpdate((to, from) => {
  if (to.name === from.name && 'id' in to.params) {
    fetchPageData(Number(to.params.id))
  }
})

fetchPageData(Number(route.params.id))

const { togglePageLoading } = usePageLoading()

watchEffect(() => {
  togglePageLoading(commitingCharacterCharacteristics.value || respecializingCharacter.value)
})
</script>

<template>
  <div class="relative mx-auto max-w-4xl">
    <div class="statsGrid grid items-start gap-6">
      <CharacterCharacteristicsBuilder
        :get-input-props
        :characteristics
        :check-current-skill-requirements-satisfied="currentSkillRequirementsSatisfied"
        :convert-attributes-to-skills-state="{ disabled: !canConvertAttributesToSkills, loading: convertingCharacterCharacteristics }"
        :convert-skills-to-attributes-state="{ disabled: !canConvertSkillsToAttributes, loading: convertingCharacterCharacteristics }"
        @input="onInput"
        @convert-attributes-to-skills="onConvertCharacterCharacteristics(CHARACTERISTIC_CONVERSION.AttributesToSkills)"
        @convert-skills-to-attributes="onConvertCharacterCharacteristics(CHARACTERISTIC_CONVERSION.SkillsToAttributes)"
      />

      <CharacterStats
        style="grid-area: stats"
        :characteristics
        :weight="itemsOverallStats.weight"
        :longest-weapon-length="itemsOverallStats.longestWeaponLength"
        :health-points="healthPoints"
      />
    </div>

    <div
      class="
        sticky bottom-0 left-0 flex w-full max-w-4xl items-center justify-center gap-4 py-3
        backdrop-blur-sm
      "
    >
      <UButton
        :disabled="!wasChangeMade"
        color="secondary"
        variant="outline"
        size="lg"
        icon="crpg:reset"
        :label="$t('action.reset')"
        data-aq-reset-action
        @click="resetCharacterCharacteristicBuilder"
      />

      <AppConfirmActionPopover @confirm="onCommitCharacterCharacteristics">
        <UButton
          size="lg"
          icon="crpg:check"
          :disabled="!wasChangeMade || !isChangeValid"
          :label="$t('action.commit')"
          data-aq-commit-action
        />
      </AppConfirmActionPopover>

      <USeparator orientation="vertical" class="h-8" />

      <CharacterActionRespec
        :character
        :respec-capability
        @respec="onRespecializeCharacter"
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
