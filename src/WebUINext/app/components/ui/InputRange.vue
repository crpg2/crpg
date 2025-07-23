<script setup lang="ts">
import { clamp } from 'es-toolkit'

const { step = 1, min, max } = defineProps<{
  min: number
  max: number
  step?: number
}>()

const modelValue = defineModel<[number, number]>({ default: () => [] })

const localValue = computed<[number, number]>({
  get() {
    let [from, to] = modelValue.value

    const hasFrom = from != null
    const hasTo = to != null

    if (!hasFrom && !hasTo) {
      return [min, max]
    }

    from = hasFrom ? Math.max(from!, min) : min
    to = hasTo ? Math.min(to!, max) : max

    return [from, to]
  },

  set([rawFrom, rawTo]) {
    let from = clamp(rawFrom, min, max)
    let to = clamp(rawTo, min, max)

    if (from > to) {
      [from, to] = [to, from]
    }

    modelValue.value = [from, to]
  },
})

const change = (from: number, to: number) => {
  localValue.value = [from, to]
}
</script>

<template>
  <div class="space-y-3">
    <div>
      <USlider
        v-model="localValue"
        :min
        :max
        :step
      />
      <div class="mt-3 flex justify-between">
        <div class="text-2xs text-muted">
          {{ $n(min) }}
        </div>
        <div class="text-2xs text-muted">
          {{ $n(max) }}
        </div>
      </div>
    </div>

    <div class="flex w-full justify-between gap-4">
      <UInputNumber
        class="w-full"
        :min
        :max
        :step
        color="neutral"
        :model-value="localValue[0]"
        @update:model-value="(value) => change(value, localValue[1])"
      />
      <UInputNumber
        class="w-full"
        :min
        :max
        :step
        color="neutral"
        :model-value="localValue[1]"
        @update:model-value="(value) => change(localValue[0], value)"
      />
    </div>
  </div>
</template>
