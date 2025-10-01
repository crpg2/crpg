<!-- ref: https://github.com/nuxt/image/blob/main/src/runtime/components/NuxtImg.vue -->
<script setup lang="ts">
import type { ImgHTMLAttributes } from 'vue'

import { onMounted, useTemplateRef } from 'vue'

const props = defineProps<{
  src?: string
  custom?: boolean
}>()

const emit = defineEmits<{
  (event: 'load', payload: Event): unknown
  (event: 'error', payload: string | Event): unknown
}>()

defineSlots<{ default: (props: {
  imgAttrs: ImgHTMLAttributes
  isLoaded: boolean
  src?: string
}) => any }>()

const placeholderLoaded = ref(false)

// @ts-expect-error TODO:
const imgEl = useTemplateRef('imgEl')

const imgAttrs = useAttrs() as ImgHTMLAttributes

onMounted(() => {
  if (props.custom) {
    const img = new Image()

    if (props.src) {
      img.src = props.src
    }

    img.onload = (event) => {
      placeholderLoaded.value = true
      emit('load', event)
    }

    img.onerror = (event) => {
      emit('error', event)
    }

    return
  }

  if (!imgEl.value) {
    return
  }

  imgEl.value.onload = (event: Event) => {
    emit('load', event)
  }

  imgEl.value.onerror = (event: Event) => {
    emit('error', event)
  }
})
</script>

<template>
  <img
    v-if="!custom"
    ref="imgEl"
    v-bind="imgAttrs"
    :src="src"
  >
  <slot
    v-else
    v-bind="{
      imgAttrs,
      isLoaded: placeholderLoaded,
      src,
    }"
  />
</template>
