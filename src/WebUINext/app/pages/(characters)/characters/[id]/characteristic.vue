<script setup lang="ts">
import { timeout } from 'es-toolkit'

import type { CharacterCharacteristics } from '~/models/character'

import { useCharacter } from '~/composables/character/use-character'
import { useCharacterCharacteristicBuilder, useCharacterCharacteristicProvider } from '~/composables/character/use-character-characteristic'
import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePageLoading } from '~/composables/utils/use-page-loading'
import { CharacteristicConversion } from '~/models/character'
import {
  computeHealthPoints,
  convertCharacterCharacteristics,
  createEmptyCharacteristic,
  getCharacterCharacteristics,
  updateCharacterCharacteristics,
} from '~/services/character-service'

// import {
//   characterItemsStatsKey,
// } from '~/symbols/character'

// const itemsStats = injectStrict(characterItemsStatsKey)

const route = useRoute('characters-id-characteristic')
const { t } = useI18n()
const toast = useToast()

const { character } = useCharacter()

const {
  state: characterCharacteristics,
  execute: loadCharacterCharacteristics,
} = useAsyncState(
  (id: number) => getCharacterCharacteristics(id),
  createEmptyCharacteristic(),
  { immediate: false, resetOnExecute: false },
)

useCharacterCharacteristicProvider(characterCharacteristics)

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

const healthPoints = computed(() => computeHealthPoints(characteristics.value.skills.ironFlesh, characteristics.value.attributes.strength))

const {
  execute: onConvertCharacterCharacteristics,
  loading: convertingCharacterCharacteristics,
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
  loading: commitingCharacterCharacteristics,
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
  loading: respecializingCharacter,
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
    <!-- eslint-disable-next-line tailwindcss/no-custom-classname -->
    <div class="statsGrid grid gap-6">
      <CharacterCharacteristicsBuilder
        :get-input-props
        :characteristics
        :check-current-skill-requirements-satisfied="currentSkillRequirementsSatisfied"
        :convert-attributes-to-skills-state="{ disabled: !canConvertAttributesToSkills, loading: convertingCharacterCharacteristics }"
        :convert-skills-to-attributes-state="{ disabled: !canConvertSkillsToAttributes, loading: convertingCharacterCharacteristics }"
        @input="onInput"
        @convert-attributes-to-skills="onConvertCharacterCharacteristics(CharacteristicConversion.AttributesToSkills)"
        @convert-skills-to-attributes="onConvertCharacterCharacteristics(CharacteristicConversion.SkillsToAttributes)"
      />

      <div
        class="grid gap-2 self-start rounded-xl border border-border-200 py-2 text-2xs"
        style="grid-area: stats"
      >
        <!-- :weight="itemsStats.weight"
      :longest-weapon-length="itemsStats.longestWeaponLength" -->
        <CharacterStats
          :characteristics
          :weight="22"
          :longest-weapon-length="33"
          :health-points="healthPoints"
        />
      </div>
    </div>

    <div class="sticky bottom-0 left-0 flex w-full max-w-4xl items-center justify-center gap-4 py-3 backdrop-blur-sm">
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

      <AppConfirmActionTooltip @confirm="onCommitCharacterCharacteristics">
        <UButton
          size="lg"
          icon="crpg:check"
          :disabled="!wasChangeMade || !isChangeValid"
          :label="$t('action.commit')"
          data-aq-commit-action
        />
      </AppConfirmActionTooltip>

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
