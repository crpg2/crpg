<script setup lang="ts">
import NumberFlow, { NumberFlowGroup } from '@number-flow/vue'
import { useCountdown } from '@vueuse/core'
import { AppHHTooltip } from '#components'

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

const hh = computed(() => Math.floor(remaining.value / 3600))
const mm = computed(() => Math.floor((remaining.value % 3600) / 60))
const ss = computed(() => remaining.value % 60)
</script>

<template>
  <UBanner
    id="hh"
    :ui="{
      title: 'text-highlighted text-lg',
    }"
    color="success"
  >
    <template #title>
      <AppHHTooltip :region>
        <div class="flex items-center gap-2">
          🎉
          <NumberFlowGroup>
            <div
              style="--number-flow-char-height: 1.85em"
              class="font-semibold tabular-nums"
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
    </template>
  </UBanner>
</template>
