<script setup lang="ts">
import type { CharacterCharacteristics, CharacterOverallItemsStats } from '~/models/character'

import { computeMountSpeedStats, computeSpeedStats, computeWeaponLengthMountPenalty } from '~/services/character-service'

type Rows = 'weight' | 'hp'

const {
  characteristics,
  itemsOverallStats,
  healthPoints,
  hiddenRows = [],
} = defineProps<{
  characteristics: CharacterCharacteristics
  itemsOverallStats: CharacterOverallItemsStats
  hiddenRows?: Rows[]
  healthPoints: number
}>()

const speedStats = computed(() =>
  computeSpeedStats({
    agility: characteristics.attributes.agility,
    strength: characteristics.attributes.strength,
    athletics: characteristics.skills.athletics,
    longestWeaponLength: itemsOverallStats.longestWeaponLength,
    totalEncumbrance: itemsOverallStats.weight,
  }))

const mountSpeedStats = computed(() => {
  if (itemsOverallStats.mountSpeedBase === 0) {
    return null
  } // unmounted
  return computeMountSpeedStats(
    itemsOverallStats.mountSpeedBase,
    itemsOverallStats.mountHarnessWeight,
    speedStats.value.perceivedWeight,
  )
})

const mountedWeaponPenalty = computed(() => {
  if (itemsOverallStats.mountSpeedBase === 0) {
    return null
  } // unmounted

  return computeWeaponLengthMountPenalty(
    itemsOverallStats.longestWeaponLength,
    characteristics.attributes.strength,
  )
})
</script>

<template>
  <UCard
    :ui="{
      body: '!px-0 overflow-hidden !py-1.5',
    }"
  >
    <div class="flex flex-col">
      <slot name="leading" />

      <UiSimpleTableRow
        :label="$t('character.stats.hp.title')"
        :value="$n(healthPoints)"
      />

      <UiSimpleTableRow
        v-if="!hiddenRows.includes('weight')"
        :label="$t('character.stats.weight.title')"
        :value="$n(itemsOverallStats.weight, 'decimal')"
      />

      <UiSimpleTableRow
        :label="$t('character.stats.freeWeight.title')"
        :value="
          `${$n(Math.min(itemsOverallStats.weight, speedStats.freeWeight), 'decimal')
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
        v-if="!isNaN(speedStats.timeToMaxSpeed)"
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
        v-if="!isNaN(speedStats.maxWeaponLength)"
        :label="$t('character.stats.maxWeaponLength.title')"
        :value="$n(speedStats.maxWeaponLength, 'decimal')"
        :tooltip="{
          title: $t('character.stats.maxWeaponLength.title'),
          description: $t('character.stats.maxWeaponLength.desc'),
        }"
      />

      <UiSimpleTableRow
        v-if="!isNaN(speedStats.movementSpeedPenaltyWhenAttacking)"
        :label="$t('character.stats.movementSpeedPenaltyWhenAttacking.title')"
        :tooltip="{
          title: $t('character.stats.movementSpeedPenaltyWhenAttacking.title'),
          description: $t('character.stats.movementSpeedPenaltyWhenAttacking.desc'),
        }"
      >
        <div
          :class="[{ 'text-error': speedStats.movementSpeedPenaltyWhenAttacking !== 0 }]"
        >
          {{ $n(speedStats.movementSpeedPenaltyWhenAttacking / 100, 'percent') }}
        </div>
      </UiSimpleTableRow>

      <UiSimpleTableRow
        v-if="mountSpeedStats"
        :label="$t('character.stats.mountSpeedPenalty.title')"
        :tooltip="{
          title: $t('character.stats.mountSpeedPenalty.title'),
          description: $t('character.stats.mountSpeedPenalty.desc'),
        }"
      >
        <div :class="[{ 'text-error': mountSpeedStats.speedReduction !== 0 }]">
          {{ $n(mountSpeedStats.speedReduction, 'percent') }}
        </div>
      </UiSimpleTableRow>

      <UiSimpleTableRow
        v-if="mountedWeaponPenalty"
        :label="$t('character.stats.mountedWeaponPenalty.title')"
        :tooltip="{
          title: $t('character.stats.mountedWeaponPenalty.title'),
          description: $t('character.stats.mountedWeaponPenalty.desc'),
        }"
      >
        <div :class="[{ 'text-error': mountedWeaponPenalty !== 1 }]">
          {{ $n(mountedWeaponPenalty - 1, 'percent') }}
        </div>
      </UiSimpleTableRow>
    </div>
  </UCard>
</template>
