<script setup lang="ts">
import NumberFlow, { NumberFlowGroup } from '@number-flow/vue'
import { useCountdown } from '@vueuse/core'

import type { Region } from '~/models/region'
import type { HHEvent } from '~/services/hh-service'

import { getHHEventRemainingSeconds } from '~/services/hh-service'

const { hHEvent } = defineProps<{ region: Region, hHEvent: HHEvent }>()

const emit = defineEmits<{ complete: [] }>()

const { remaining } = useCountdown(
  () => getHHEventRemainingSeconds(hHEvent),
  {
    immediate: true,
    onComplete: () => {
      emit('complete')
    },
  },
)

const time = computed(() => ({
  hh: Math.floor(remaining.value / 3600),
  mm: Math.floor((remaining.value % 3600) / 60),
  ss: remaining.value % 60,
}))
</script>

<template>
  <UBadge variant="outline" size="xl">
    <AppHHTooltip :region>
      <div class="flex items-center gap-2">
        🎉
        <NumberFlowGroup>
          <div class="items-baseline font-semibold tabular-nums">
            <NumberFlow
              :trend="-1"
              :value="time.hh"
              :format="{ minimumIntegerDigits: 2 }"
            />
            <NumberFlow
              prefix=":"
              :trend="-1"
              :value="time.mm"
              :digits="{ 1: { max: 5 } }"
              :format="{ minimumIntegerDigits: 2 }"
            />
            <NumberFlow
              prefix=":"
              :trend="-1"
              :value="time.ss"
              :digits="{ 1: { max: 5 } }"
              :format="{ minimumIntegerDigits: 2 }"
            />
          </div>
        </NumberFlowGroup>
        🎉
      </div>
    </AppHHTooltip>
  </UBadge>
</template>
