<script setup lang="ts">
import { useCharacter } from '~/composables/character/use-character'
import { useCharacterCharacteristic, useCharacterCharacteristicBuilder } from '~/composables/character/use-character-characteristic'
import { useCharacterItems } from '~/composables/character/use-character-items'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { CHARACTERISTIC_CONVERSION } from '~/models/character'

const { character, characterId } = useCharacter()

const {
  characterCharacteristics,
  loadCharacterCharacteristics,
  convertingCharacterCharacteristics,
  onCommitCharacterCharacteristics,
  onConvertCharacterCharacteristics,
} = useCharacterCharacteristic(characterId)

const { itemsOverallStats } = useCharacterItems(characterId)

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
  healthPoints,
} = useCharacterCharacteristicBuilder(characterCharacteristics)

const { respecCapability, onRespecializeCharacter } = useCharacterRespec(loadCharacterCharacteristics)
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
        color="neutral"
        variant="outline"
        size="xl"
        icon="crpg:reset"
        :label="$t('action.reset')"
        data-aq-reset-action
        @click="resetCharacterCharacteristicBuilder"
      />

      <AppConfirmActionPopover
        @confirm="() => {
          onCommitCharacterCharacteristics(characteristics)
          resetCharacterCharacteristicBuilder()
        }"
      >
        <UButton
          size="xl"
          icon="crpg:check"
          :disabled="!wasChangeMade || !isChangeValid"
          :label="$t('action.commit')"
          data-aq-commit-action
        />
      </AppConfirmActionPopover>

      <div>
        <CharacterActionRespec
          :character
          :respec-capability
          @respec="onRespecializeCharacter"
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
