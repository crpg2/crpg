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

defineSlots<{
  'filter': () => any
  'label-trailing'?: () => any
  'filter-content'?: (props: { open: boolean, toggle: (state: boolean) => void }) => any
}>()

const [open, toggle] = useToggle()
</script>

<template>
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
          v-model:open="open"
          :ui="{ content: 'max-w-76' }"
        >
          <UiGridColumnHeaderLabel :with-filter :label :description />

          <template #content>
            <slot name="filter-content" v-bind="{ open, toggle }" />
          </template>
        </UPopover>
      </slot>
    </template>

    <template v-else>
      <UiGridColumnHeaderLabel
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
