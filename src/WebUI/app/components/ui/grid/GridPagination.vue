<script setup lang="ts" generic="TData">
const { page, size, total } = defineProps<{
  page: number
  size: number
  total: number
}>()

defineEmits<{
  'update:page': [page: number]
}>()

const counter = computed(() => [
  Math.min(size * (page - 1) + 1, total),
  Math.min(size * page, total),
])
</script>

<template>
  <div class="grid grid-cols-3 items-center gap-4">
    <!-- :default-page="tableApi.value.initialState.pagination.pageIndex + 1" -->
    <UPagination
      variant="soft"
      active-variant="subtle"
      active-color="primary"
      :page
      :show-controls="false"
      show-edges
      size="xl"
      :items-per-page="size"
      :total
      :ui="{
        root: 'py-2',
      }"
      @update:page="$emit('update:page', $event)"
    />

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
