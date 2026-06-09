<script lang="ts" setup>
import type { SelectedItem } from '~/models/item'

defineProps<{
  item: SelectedItem | null
  label: string
  disabled?: boolean
}>()

defineEmits<{
  clear: []
}>()
</script>

<template>
  <UButton
    block
    variant="outline"
    color="neutral"
    :ui="{
      base: 'justify-between',
    }"
    :disabled
  >
    <ItemMedia
      v-if="item"
      :base-id="item.baseId"
      :rank="item.rank"
      :name="item.name"
    />
    <ItemMedia
      v-else
      base-id="placeholder"
      :rank="0"
      :name="label"
    />

    <template #trailing>
      <div class="flex items-center gap-1">
        <UButton
          v-if="item"
          as="span"
          variant="ghost"
          color="neutral"
          icon="i-lucide-x"
          @click.stop="$emit('clear')"
        />
        <UIcon name="i-lucide-chevron-down" class="size-5" />
      </div>
    </template>
  </UButton>
</template>
