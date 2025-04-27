<script setup lang="ts">
import type { CharacterCharacteristics } from '~/models/character'

import { computeSpeedStats } from '~/services/character-service'

type Rows = 'weight' | 'hp'

const props = withDefaults(
  defineProps<{
    characteristics: CharacterCharacteristics
    weight: number
    longestWeaponLength: number
    healthPoints: number
    hiddenRows?: Rows[]
  }>(),
  {
    hiddenRows: () => [],
  },
)

const speedStats = computed(() =>
  computeSpeedStats(
    props.characteristics.attributes.strength,
    props.characteristics.skills.athletics,
    props.characteristics.attributes.agility,
    props.weight,
    props.longestWeaponLength,
  ),
)
</script>

<template>
  <UCard
    :ui="{ root: 'overflow-hidden', body: '!p-0 overflow-hidden', header: '!px-4 py-3' }"
  >
    <!-- <template #header>
      <UiDataCell class="w-full text-sm">
        Stats
      </UiDataCell>
    </template> -->

    <div class="flex flex-col text-2xs">
      <slot name="leading" />

      <UiSimpleTableRow
        :label="$t('character.stats.hp.title')"
        :value="$n(healthPoints)"
      />

      <UiSimpleTableRow
        v-if="!hiddenRows.includes('weight')"
        :label="$t('character.stats.weight.title')"
        :value="$n(weight, 'decimal')"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.freeWeight.title')"
        :value="
          `${$n(Math.min(weight, speedStats.freeWeight), 'decimal')
          }/${
            $n(speedStats.freeWeight, 'decimal')}`
        "
        :tooltip="{
          title: $t('character.stats.freeWeight.title'),
          description: $t('character.stats.freeWeight.desc'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.weightReduction.title')"
        :value="$n(speedStats.weightReductionFactor - 1, 'percent')"
        :tooltip="{
          title: $t('character.stats.weightReduction.title'),
          description: $t('character.stats.weightReduction.desc'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.perceivedWeight.title')"
        :value="$n(speedStats.perceivedWeight, 'decimal')"
        :tooltip="{
          title: $t('character.stats.perceivedWeight.title'),
          description: $t('character.stats.perceivedWeight.desc'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.timeToMaxSpeed.title')"
        :value="$n(speedStats.timeToMaxSpeed, 'second')"
        :tooltip="{
          title: $t('character.stats.timeToMaxSpeed.title'),
          description: $t('character.stats.timeToMaxSpeed.desc'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.nakedSpeed.title')"
        :value="$n(speedStats.nakedSpeed, 'decimal')"
        :tooltip="{
          title: $t('character.stats.nakedSpeed.title'),
          description: $t('character.stats.nakedSpeed.desc'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.currentSpeed.title')"
        :value="$n(speedStats.currentSpeed, 'decimal')"
        :tooltip="{
          title: $t('character.stats.currentSpeed.title'),
          description: $t('character.stats.currentSpeed.desc'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.maxWeaponLength.title')"
        :value="$n(speedStats.maxWeaponLength, 'decimal')"
        :tooltip="{
          title: $t('character.stats.maxWeaponLength.title'),
          description: $t('character.stats.maxWeaponLength.desc'),
        }"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.movementSpeedPenaltyWhenAttacking.title')"
        :tooltip="{
          title: $t('character.stats.movementSpeedPenaltyWhenAttacking.title'),
          description: $t('character.stats.movementSpeedPenaltyWhenAttacking.desc'),
        }"
      >
        <div
          class="text-xs"
          :class="[
            speedStats.movementSpeedPenaltyWhenAttacking !== 0
              ? 'text-status-danger'
              : 'text-content-100',
          ]"
        >
          {{ $n(speedStats.movementSpeedPenaltyWhenAttacking / 100, 'percent') }}
        </div>
      </UiSimpleTableRow>
    </div>
  </UCard>
</template>
