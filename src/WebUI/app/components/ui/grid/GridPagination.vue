<script setup lang="ts">
import type { Table } from '@tanstack/table-core'

const { tableApi } = defineProps<{
  tableApi: Ref<Table<any>> // TODO: нужно убрать ref
}>()

const total = computed(() => tableApi.value.getRowCount())
const page = computed(() => tableApi.value.getState().pagination.pageIndex + 1)
const pageSize = computed(() => tableApi.value.getState().pagination.pageSize)

const counter = computed(() => [
  Math.min(pageSize.value * (page.value - 1) + 1, total.value),
  Math.min(pageSize.value * page.value, total.value),
])
</script>

<template>
  <div class="grid grid-cols-3 items-center gap-4">
    <div>
      <UPagination
        variant="soft"
        active-variant="subtle"
        active-color="primary"
        :page
        :show-controls="false"
        show-edges
        size="xl"
        :default-page="tableApi.value.initialState.pagination.pageIndex + 1"
        :items-per-page="pageSize"
        :total
        @update:page="(value) => tableApi.value.setPageIndex(value - 1)"
      />
    </div>

    <div class="flex justify-center">
      <slot />
    </div>

    <div class="flex justify-end">
      <UiTextView v-if="total !== 0 && total !== Infinity" variant="caption">
        {{ $t('pagination.counter', { from: counter[0], to: counter[1], total }) }}
      </UiTextView>
    </div>
  </div>
</template>
