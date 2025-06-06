<script setup lang="ts">
defineProps<{ value?: number }>()
</script>

<template>
  <UTooltip :text="$t('user.field.gold')">
    <UiDataCell>
      <template #leftContent>
        <UiSpriteSymbol name="coin" viewBox="0 0 18 18" class="size-4" />
      </template>
      <slot>
        <span v-if="value !== undefined" class="font-bold text-primary">
          {{ $n(value) }}
        </span>
      </slot>
    </UiDataCell>
  </UTooltip>
</template>
