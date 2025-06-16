<script setup lang="ts">
import NumberFlow, { NumberFlowGroup } from '@number-flow/vue'
import { useCountdown } from '@vueuse/core'

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

const { remaining } = useCountdown(() => getHHEventRemaining(HHEvent.value), {
  immediate: true,
})

const hh = computed(() => Math.floor(remaining.value / 3600))
const mm = computed(() => Math.floor((remaining.value % 3600) / 60))
const ss = computed(() => remaining.value % 60)
</script>

<template>
  <div class="bg-success px-8 py-1">
    <AppHHTooltip :region>
      <div
        class="
          flex cursor-pointer items-center justify-center gap-2 text-sm font-semibold
          text-highlighted
        "
      >
        🎉
        <NumberFlowGroup>
          <div
            style="font-variant-numeric: tabular-nums; --number-flow-char-height: 0.85em"
            class="font-semibold"
          >
            <NumberFlow
              :trend="-1"
              :value="hh"
              :format="{ minimumIntegerDigits: 2 }"
            />
            <NumberFlow
              prefix=":"
              :trend="-1"
              :value="mm"
              :digits="{ 1: { max: 5 } }"
              :format="{ minimumIntegerDigits: 2 }"
            />
            <NumberFlow
              prefix=":"
              :trend="-1"
              :value="ss"
              :digits="{ 1: { max: 5 } }"
              :format="{ minimumIntegerDigits: 2 }"
            />
          </div>
        </NumberFlowGroup>
        🎉
      </div>
    </AppHHTooltip>
  </div>
</template>
