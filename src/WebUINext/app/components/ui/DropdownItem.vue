<script setup lang="ts">
import type { NuxtLinkProps } from '#app'

import { NuxtLink } from '#components'

const { active = false, checked = false } = defineProps<{
  active?: boolean
  checked?: boolean
  link?: NuxtLinkProps
  label?: string
  icon?: string
}>()
</script>

<template>
  <component
    :is="link ? NuxtLink : 'div'"
    v-bind="{ ...(link && link) }"
    class="flex cursor-pointer flex-wrap items-center gap-3 bg-base-200 px-5 py-3"
    :class="[
      active || checked
        ? 'text-content-100 hover:text-content-200'
        : 'text-content-300 hover:text-content-100',
    ]"
  >
    <slot>
      <OIcon
        v-if="icon"
        :icon="icon"
        size="sm"
      />
      <div v-if="label">
        {{ label }}
      </div>
    </slot>

    <FontAwesomeLayers
      v-if="checked"
      class="fa-lg"
    >
      <FontAwesomeIcon
        :icon="['crpg', 'fa-circle']"
        size="lg"
        class="text-content-100"
      />
      <FontAwesomeIcon
        :icon="['crpg', 'fa-check']"
        size="sm"
        class="text-base-200"
      />
    </FontAwesomeLayers>
  </component>
</template>
