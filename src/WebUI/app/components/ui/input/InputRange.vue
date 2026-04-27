<script setup lang="ts">
import { refDebounced } from '@vueuse/core'
import { clamp } from 'es-toolkit'

const {
  min,
  max,
  step = 1,
  debounce = 300,
} = defineProps<{
  min: number
  max: number
  step?: number
  debounce?: number
}>()

const modelValue = defineModel<[number, number]>({ default: () => [] })

const localValue = ref<[number, number]>([min, max])

const change = (from: number, to: number) => {
  localValue.value = [from, to]
}

const debouncedValue = refDebounced(localValue, debounce)

// sync debouncedValue -> modelValue
watch(debouncedValue, (val) => {
  let [from, to] = val
  from = clamp(from, min, max)
  to = clamp(to, min, max)
  if (from > to) {
    [from, to] = [to, from]
  }
  modelValue.value = [from, to]
}, { deep: true })

// sync modelValue -> localValue
watchEffect(() => {
  let [from, to] = modelValue.value
  const hasFrom = from != null
  const hasTo = to != null

  if (!hasFrom && !hasTo) {
    localValue.value = [min, max]
    return
  }

  from = hasFrom ? Math.max(from!, min) : min
  to = hasTo ? Math.min(to!, max) : max
  if (from > to) {
    [from, to] = [to, from]
  }

  localValue.value = [from, to]
})
</script>

<template>
  <div class="space-y-3">
    <div class="pt-2">
      <USlider
        v-model="localValue"
        :min
        :max
        :step
      />
      <div class="mt-2.5 flex justify-between">
        <div class="text-xs text-muted">
          {{ $n(min) }}
        </div>
        <div class="text-xs text-muted">
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
        :model-value="localValue[0]"
        @update:model-value="(value) => change(value!, localValue[1])"
      />
      <UInputNumber
        class="w-full"
        :min
        :max
        :step
        :model-value="localValue[1]"
        @update:model-value="(value) => change(localValue[0], value!)"
      />
    </div>
  </div>
</template>
