<script setup lang="ts">
import type { TextViewVariant } from '~/components/ui/text/TextView.vue'

const { tag = 'h1', variant = 'h1' } = defineProps<{
  title?: string
  tag?: string
  variant?: TextViewVariant
}>()
</script>

<template>
  <div class="flex flex-col items-center gap-5">
    <slot name="icon" />

    <div
      class="
        flex items-center justify-center gap-4
        md:gap-8
      "
    >
      <UiSpriteSymbol
        name="logo-decor"
        viewBox="0 0 108 10"
        class="
          w-16 rotate-180
          md:w-28
        "
      />

      <slot>
        <UiTextView v-if="title" :tag :variant class="text-center">
          {{ title }}
        </UiTextView>
      </slot>

      <UiSpriteSymbol
        name="logo-decor"
        viewBox="0 0 108 10"
        class="
          w-16
          md:w-28
        "
      />
    </div>
  </div>
</template>
