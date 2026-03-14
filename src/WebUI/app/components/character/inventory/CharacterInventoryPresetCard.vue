<script setup lang="ts">
import type { UserItemPreset } from '~/models/user'

import { getItemImage } from '~/services/item-service'

const { preset } = defineProps<{
  preset: UserItemPreset
}>()

defineEmits<{
  apply: []
  delete: []
}>()
</script>

<template>
  <UCard
    :ui="{
      header: 'justify-between flex gap-4 py-2! items-center',
      body: 'py-2',
    }"
  >
    <template #header>
      <UiTextView variant="h5">
        {{ preset.name }}
      </UiTextView>

      <div class="flex flex-wrap gap-2">
        <UButton
          color="success"
          variant="ghost"
          icon="i-lucide-check"
          :label="$t('action.apply')"
          @click="$emit('apply')"
        />

        <UTooltip :text="$t('character.inventory.presets.delete')">
          <UButton
            color="error"
            variant="ghost"
            icon="i-lucide-trash-2"
            @click="$emit('delete')"
          />
        </UTooltip>
      </div>
    </template>

    <div class="grid grid-cols-5 gap-3">
      <ItemCard
        v-for="slot in preset.slots.filter(slot => slot.item)"
        :key="slot.slot"
        :item="slot.item!"
        class="aspect-4/3 h-auto"
      >
        <template v-if="!slot.userItemId" #badges-top-right>
          <UBadge

            color="error"
            variant="soft"
            size="sm"
            icon="i-lucide-circle-alert"
            class="absolute top-0 right-0"
          />
        </template>
      </ItemCard>
    </div>
  </UCard>
</template>
