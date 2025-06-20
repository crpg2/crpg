<script setup lang="ts">
defineProps<{
  thumb: string
  name: string
}>()

const error = ref<boolean>(false)
</script>

<template>
  <NuxtImg
    v-slot="{ src, isLoaded, imgAttrs }"
    :src="thumb"
    :alt="name"
    :custom="true"
    @error="error = true"
  >
    <img
      v-if="isLoaded"
      v-bind="imgAttrs"
      class="size-full object-contain select-none"
      :src="src"
    >
    <div
      v-else-if="error"
      class="
        flex size-full flex-col items-center justify-center gap-1 overflow-hidden p-2 text-center
        text-dimmed
      "
    >
      <UIcon
        name="crpg:error"
        class="size-8"
      />
      <div class="w-full truncate text-2xs">
        {{ name }}
      </div>
    </div>

    <USkeleton v-else class="size-full" />
  </NuxtImg>
</template>
