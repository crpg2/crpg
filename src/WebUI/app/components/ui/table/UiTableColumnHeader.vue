<script setup lang="ts">
type SortDirection = 'asc' | 'desc'

withDefaults(defineProps<{
  label: string
  description?: string
  withSort?: boolean
  sorted?: false | SortDirection
  withFilter?: boolean
  filtered?: boolean
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
  <!-- <UFieldGroup>
    <UButton
      v-if="withFilter"
      variant="soft"
      color="neutral"
      icon="crpg:chevron-down"
      @click="$emit('resetFilter')"
    />

    <UButton
      variant="soft"
      color="neutral"
      :label
    />

    <UButton
      v-if="withSort"
      variant="soft"
      color="neutral"
      :icon="sorted
        ? (sorted === 'asc'
          ? 'crpg:arrow-up-narrow-wide'
          : 'crpg:arrow-down-narrow-wide')
        : 'crpg:arrow-up-down'"
      @click="$emit('sort')"
    />
  </UFieldGroup> -->

  <div class="relative flex items-center gap-1">
    <UTooltip
      v-if="withFilter && filtered"
      :text="$t('action.reset')"
    >
      <UButton
        class="absolute top-1/2 -left-7 -translate-y-1/2"
        square
        color="neutral"
        variant="ghost"
        size="xs"
        icon="crpg:close"
        @click="$emit('resetFilter')"
      />
    </UTooltip>

    <template v-if="withFilter">
      <slot name="filter">
        <UPopover
          :ui="{ content: 'max-w-76' }"
        >
          <UiTableColumnHeaderLabel :with-filter :label :description />

          <template #content>
            <slot name="filter-content" />
          </template>
        </UPopover>
      </slot>
    </template>

    <template v-else>
      <UiTableColumnHeaderLabel
        :with-filter="false"
        :label
        :description
      />
      <slot name="label-trailing" />
    </template>

    <UTooltip
      v-if="withSort"
      :text="$t(sorted === 'asc' ? 'sort.directions.desc' : 'sort.directions.asc')"
    >
      <UButton
        square
        color="neutral"
        variant="ghost"
        size="sm"
        :icon="sorted
          ? (sorted === 'asc'
            ? 'crpg:arrow-up-narrow-wide'
            : 'crpg:arrow-down-narrow-wide')
          : 'crpg:arrow-up-down'"

        @click="$emit('sort')"
      />
    </UTooltip>
  </div>
</template>
