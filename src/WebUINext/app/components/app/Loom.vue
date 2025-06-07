<script setup lang="ts">
const { point } = defineProps<{ point?: number }>()
</script>

<template>
  <UTooltip :text="$t('user.field.heirloom')" class="text-primary">
    <UiDataCell>
      <template #leftContent>
        <div class="flex h-5 items-center">
          <UIcon name="crpg:blacksmith" class="size-4" />
        </div>
      </template>
      <slot>
        <span v-if="point !== undefined" class="font-bold">
          {{ $n(point) }}
        </span>
      </slot>
    </UiDataCell>
  </UTooltip>
</template>
