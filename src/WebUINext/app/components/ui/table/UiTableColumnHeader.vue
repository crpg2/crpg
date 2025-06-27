<script setup lang="ts">
import type { DropdownMenuProps } from '@nuxt/ui'

type SortDirection = 'asc' | 'desc'

withDefaults(defineProps<{
  label: string
  withFilter?: boolean
  withSort?: boolean
  sorted?: false | SortDirection
  filtered?: boolean
  filterDropdownItems?: DropdownMenuProps['items']
}>(), {
  withSort: false,
  withFilter: false,
})

defineEmits<{
  sort: []
  resetFilter: []
}>()
</script>

<template>
  <div class="relative flex items-center gap-1">
    <UTooltip
      v-if="withFilter && filtered"
      :text="$t('action.reset')"
    >
      <UIcon
        class="
          absolute top-1/2 -left-5 size-4 -translate-y-1/2 cursor-pointer
          text-muted outline-0 select-none
          hover:text-toned
        "
        name="crpg:close"
        @click="$emit('resetFilter')"
      />
    </UTooltip>

    <UDropdownMenu
      v-if="withFilter"
      size="xl"
      :modal="false"
      :items="filterDropdownItems"
    >
      <div
        class="
          cursor-pointer text-xs underline decoration-dashed underline-offset-6
          select-none
          hover:no-underline
        "
      >
        {{ label }}
      </div>
    </UDropdownMenu>

    <div
      v-else
      class="text-xs"
    >
      {{ label }}
    </div>

    <!-- <UPopover>
      <div
        :class="{ 'cursor-pointer text-2xs underline decoration-dashed underline-offset-6 select-none hover:no-underline': withFilter }"
      >
        {{ label }}
      </div>

      <template #content>
        <div class="max-h-64 max-w-md overflow-y-auto">
          <slot name="filter-dropdown" />
        </div>
      </template>
    </UPopover> -->

    <UTooltip v-if="withSort" :text="$t(sorted === 'asc' ? 'shop.sort.desc' : 'shop.sort.asc')">
      <div
        class="
          flex cursor-pointer flex-col text-muted
          hover:text-toned
        "
        @click="$emit('sort')"
      >
        <UIcon
          v-if="!sorted || sorted === 'asc'"
          class="-my-[0.25rem] size-4"
          name="crpg:chevron-up"
        />
        <UIcon
          v-if="!sorted || sorted === 'desc'"
          class="-my-[0.25rem] size-4"
          name="crpg:chevron-down"
        />
      </div>
    </UTooltip>
  </div>
</template>
