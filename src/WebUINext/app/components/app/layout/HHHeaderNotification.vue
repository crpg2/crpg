<script setup lang="ts">
import NumberFlow, { NumberFlowGroup } from '@number-flow/vue'

import type { Region } from '~/models/region'

import { useHappyHours } from '~/composables/use-hh'
import { getHHEventRemaining } from '~/services/hh-service'

defineProps<{ region: Region }>()

const {
  HHEvent,
  // HHEventRemaining,
  onEndHHCountdown,
  onStartHHCountdown,
} = useHappyHours()

const HHEventRemaining = computed(() => getHHEventRemaining(HHEvent.value))
</script>

<template>
  <div class="flex items-center justify-center bg-status-success px-8 py-1">
    <AppHHTooltip :region>
      {{ HHEventRemaining }}
      <div class="flex-1 cursor-pointer items-center gap-2 text-sm text-content-100">
        🎉
        <NumberFlowGroup>
          <NumberFlow
            :trend="-1"
            :value="HHEventRemaining.hours"
            :format="{ minimumIntegerDigits: 2 }"
          />
          <NumberFlow
            prefix=":"
            :trend="-1"
            :value="HHEventRemaining.minutes"
            :digits="{ 1: { max: 5 } }"
            :format="{ minimumIntegerDigits: 2 }"
          />
          <NumberFlow
            prefix=":"
            :trend="-1"
            :value="HHEventRemaining.seconds"
            :digits="{ 1: { max: 5 } }"
            :format="{ minimumIntegerDigits: 2 }"
          />
        </NumberFlowGroup>
        <!-- <VueCountdown
          v-slot="{ hours, minutes, seconds }"
          class="w-24"
          :time="HHEventRemaining"
          :transform="transformSlotProps"
          @start="onStartHHCountdown"
          @end="onEndHHCountdown"
        >
          {{ $t('dateTimeFormat.countdown', { hours, minutes, seconds }) }}
        </VueCountdown> -->
        🎉
      </div>
    </AppHHTooltip>
  </div>
</template>
