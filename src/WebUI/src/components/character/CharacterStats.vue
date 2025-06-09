<script setup lang="ts">
    import { computed } from 'vue'
    import type { CharacterCharacteristics } from '~/models/character'

    import {
        computeSpeedStats,
        computeMountSpeedStats,
    } from '~/services/characters-service'

    type Rows = 'weight' | 'hp'

    const props = withDefaults(
        defineProps<{
            characteristics: CharacterCharacteristics
            weight: number
            longestWeaponLength: number
            healthPoints: number
            mountSpeedBase?: number | null
            mountHarnessWeight?: number | null
            hiddenRows?: Rows[]
        }>(),
        {
            hiddenRows: () => [],
            mountSpeedBase: null,
            mountHarnessWeight: null,
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

    const hasMount = computed(() =>
        props.mountSpeedBase != null &&
        props.mountHarnessWeight != null &&
        speedStats.value?.perceivedWeight != null
    )

    const mountSpeedStats = computed(() => {
        if (!hasMount.value) return null
        return computeMountSpeedStats(
            props.mountSpeedBase!,
            props.mountHarnessWeight!,
            speedStats.value.perceivedWeight
        )
    })
</script>

<template>
    <SimpleTableRow :label="$t('character.stats.hp.title')"
                    :value="$n(healthPoints)" />

    <SimpleTableRow v-if="!hiddenRows.includes('weight')"
                    :label="$t('character.stats.weight.title')"
                    :value="$n(weight, 'decimal')" />

    <SimpleTableRow :label="$t('character.stats.freeWeight.title')"
                    :value="`${$n(Math.min(weight, speedStats.freeWeight), 'decimal')}/${$n(speedStats.freeWeight, 'decimal')}`"
                    :tooltip="{
      title: $t('character.stats.freeWeight.title'),
      description: $t('character.stats.freeWeight.desc'),
    }" />

    <SimpleTableRow :label="$t('character.stats.weightReduction.title')"
                    :value="$n(speedStats.weightReductionFactor - 1, 'percent')"
                    :tooltip="{
      title: $t('character.stats.weightReduction.title'),
      description: $t('character.stats.weightReduction.desc'),
    }" />

    <SimpleTableRow :label="$t('character.stats.perceivedWeight.title')"
                    :value="$n(speedStats.perceivedWeight, 'decimal')"
                    :tooltip="{
      title: $t('character.stats.perceivedWeight.title'),
      description: $t('character.stats.perceivedWeight.desc'),
    }" />

    <SimpleTableRow :label="$t('character.stats.timeToMaxSpeed.title')"
                    :value="$n(speedStats.timeToMaxSpeed, 'second')"
                    :tooltip="{
      title: $t('character.stats.timeToMaxSpeed.title'),
      description: $t('character.stats.timeToMaxSpeed.desc'),
    }" />

    <SimpleTableRow :label="$t('character.stats.nakedSpeed.title')"
                    :value="$n(speedStats.nakedSpeed, 'decimal')"
                    :tooltip="{
      title: $t('character.stats.nakedSpeed.title'),
      description: $t('character.stats.nakedSpeed.desc'),
    }" />

    <SimpleTableRow :label="$t('character.stats.currentSpeed.title')"
                    :value="$n(speedStats.currentSpeed, 'decimal')"
                    :tooltip="{
      title: $t('character.stats.currentSpeed.title'),
      description: $t('character.stats.currentSpeed.desc'),
    }" />

    <SimpleTableRow :label="$t('character.stats.maxWeaponLength.title')"
                    :value="$n(speedStats.maxWeaponLength, 'decimal')"
                    :tooltip="{
      title: $t('character.stats.maxWeaponLength.title'),
      description: $t('character.stats.maxWeaponLength.desc'),
    }" />

    <SimpleTableRow :label="$t('character.stats.movementSpeedPenaltyWhenAttacking.title')"
                    :tooltip="{
      title: $t('character.stats.movementSpeedPenaltyWhenAttacking.title'),
      description: $t('character.stats.movementSpeedPenaltyWhenAttacking.desc'),
    }">
        <div class="text-xs"
             :class="[
        speedStats.movementSpeedPenaltyWhenAttacking !== 0
          ? 'text-status-danger'
          : 'text-content-100',
      ]">
            {{ $n(speedStats.movementSpeedPenaltyWhenAttacking / 100, 'percent') }}
        </div>
    </SimpleTableRow>

    <SimpleTableRow :label="$t('character.stats.mountSpeedPenalty.title')"
                    :value="mountSpeedStats ? $n(mountSpeedStats.speedReduction, 'percent') : '—'"
                    :tooltip="{
      title: $t('character.stats.mountSpeedPenalty.title'),
      description: mountSpeedStats
        ? $t('character.stats.mountSpeedPenalty.desc')
        : 'No mount equipped.',
    }" />
</template>
