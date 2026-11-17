<script setup lang="ts">
import type { SelectItem } from '@nuxt/ui'

import type { ItemTheme } from '~/models/item'

type ThemeEditMode = 'set' | 'add' | 'remove'

const { mode, themes, initialThemeIds = [], targetLabel = '', count = 0, loading = false } = defineProps<{
  mode: ThemeEditMode
  themes: ItemTheme[]
  initialThemeIds?: number[]
  targetLabel?: string
  count?: number
  loading?: boolean
}>()

const emit = defineEmits<{
  submit: [themeIds: number[]]
}>()

const open = defineModel<boolean>('open', { required: true })

const { t } = useI18n()

const selected = ref<number[]>([])

// Reset the selection whenever the modal opens: in `set` mode it is seeded with the item's current themes,
// in `add`/`remove` mode it starts empty.
watch(open, (isOpen) => {
  if (isOpen) {
    selected.value = mode === 'set' ? [...initialThemeIds] : []
  }
})

const themeItems = computed<SelectItem[]>(() => themes.map(theme => ({ label: theme.name, value: theme.id })))

const title = computed(() => {
  switch (mode) {
    case 'set':
      return t('theme.tag.edit.title', { item: targetLabel })
    case 'add':
      return t('theme.tag.bulk.add.title', { count })
    case 'remove':
      return t('theme.tag.bulk.remove.title', { count })
  }
  return ''
})

const submitLabel = computed(() => t(`theme.tag.submit.${mode}`))

// `set` accepts an empty selection (clears all themes); `add`/`remove` require at least one theme.
const canSubmit = computed(() => mode === 'set' || selected.value.length > 0)

function onSubmit() {
  if (!canSubmit.value) {
    return
  }
  emit('submit', [...selected.value])
}
</script>

<template>
  <UModal v-model:open="open" :title="title">
    <template #body>
      <div class="space-y-4">
        <USelect
          v-model="selected"
          :items="themeItems"
          multiple
          :placeholder="$t('theme.tag.field.themes.placeholder')"
          class="w-full"
        />

        <p v-if="!themes.length" class="text-sm text-muted">
          {{ $t('theme.tag.empty') }}
        </p>
      </div>
    </template>

    <template #footer>
      <div class="flex w-full items-center justify-end gap-2">
        <UButton
          color="neutral"
          variant="subtle"
          :label="$t('action.cancel')"
          :disabled="loading"
          @click="open = false"
        />
        <UButton
          :label="submitLabel"
          :loading="loading"
          :disabled="!canSubmit"
          @click="onSubmit"
        />
      </div>
    </template>
  </UModal>
</template>
