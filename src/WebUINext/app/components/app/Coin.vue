<script setup lang="ts">
defineProps<{ value?: number }>()
</script>

<template>
  <UTooltip :text="$t('user.field.gold')" class="text-primary">
    <UiDataCell>
      <template #leftContent>
        <div class="flex h-5 items-center">
          <UiSpriteSymbol name="coin" viewBox="0 0 18 18" class="size-4" />
        </div>
      </template>
      <slot>
        <span v-if="value !== undefined" class="font-bold">
          {{ $n(value) }}
        </span>
      </slot>
    </UiDataCell>
  </UTooltip>
</template>
