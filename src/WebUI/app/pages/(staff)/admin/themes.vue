<script setup lang="ts">
import type { ThemeEventFormData, ThemeEventViewModel, ThemeViewModel } from '~/models/theme'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ROLE } from '~/models/role'
import {
  createTheme,
  createThemeEvent,
  deleteTheme,
  deleteThemeEvent,
  getThemeEvents,
  getThemes,
  updateTheme,
  updateThemeEvent,
} from '~/services/theme-service'

definePageMeta({
  roles: [ROLE.Admin],
})

const { t } = useI18n()

const { state: themes, execute: loadThemes, isLoading: loadingThemes } = useAsyncState(
  () => getThemes(),
  [],
  { resetOnExecute: false },
)

const { state: events, execute: loadEvents, isLoading: loadingEvents } = useAsyncState(
  () => getThemeEvents(),
  [],
  { resetOnExecute: false },
)

const themeModalOpen = ref(false)
const editingTheme = ref<ThemeViewModel | null>(null)

const openCreateTheme = () => {
  editingTheme.value = null
  themeModalOpen.value = true
}

const openEditTheme = (theme: ThemeViewModel) => {
  editingTheme.value = theme
  themeModalOpen.value = true
}

const { execute: submitTheme, isLoading: savingTheme } = useAsyncCallback(
  async (data: { name: string }) => {
    if (editingTheme.value) {
      await updateTheme({
        ...data,
        id: editingTheme.value.id,
      })
    }
    else {
      await createTheme(data)
    }
    themeModalOpen.value = false
    await loadThemes()
  },
  { successMessage: t('theme.notify.saved') },
)

const { execute: removeTheme } = useAsyncCallback(
  async (theme: ThemeViewModel) => {
    await deleteTheme(theme.id)
    await Promise.all([loadThemes(), loadEvents()])
  },
  { successMessage: t('theme.notify.deleted') },
)

const eventModalOpen = ref(false)
const editingEvent = ref<ThemeEventViewModel | null>(null)

const openCreateEvent = () => {
  editingEvent.value = null
  eventModalOpen.value = true
}

const openEditEvent = (event: ThemeEventViewModel) => {
  editingEvent.value = event
  eventModalOpen.value = true
}

const { execute: submitEvent, isLoading: savingEvent } = useAsyncCallback(
  async (data: ThemeEventFormData) => {
    if (editingEvent.value) {
      await updateThemeEvent({
        ...data,
        id: editingEvent.value.id,
        activeFromUtc: data.activeFromUtc.toISOString() as unknown as Date,
        activeUntilUtc: data.activeUntilUtc ? data.activeUntilUtc.toISOString() as unknown as Date : null,
      })
    }
    else {
      await createThemeEvent({
        ...data,
        activeFromUtc: data.activeFromUtc.toISOString() as unknown as Date,
        activeUntilUtc: data.activeUntilUtc ? data.activeUntilUtc.toISOString() as unknown as Date : null,
      })
    }
    eventModalOpen.value = false
    await loadEvents()
  },
  { successMessage: t('theme.event.notify.saved') },
)

const { execute: removeEvent } = useAsyncCallback(
  async (event: ThemeEventViewModel) => {
    await deleteThemeEvent(event.id)
    await loadEvents()
  },
  { successMessage: t('theme.event.notify.deleted') },
)
</script>

<template>
  <UContainer class="space-y-12 py-6">
    <section class="space-y-4">
      <div class="flex items-center justify-between gap-4">
        <UiTextView variant="h2" tag="h2">
          {{ $t('theme.title') }}
        </UiTextView>

        <UButton
          variant="subtle"
          icon="crpg:plus"
          :label="$t('theme.create.title')"
          @click="openCreateTheme"
        />
      </div>

      <AdminThemesTable
        :themes="themes"
        :loading="loadingThemes"
        @edit="openEditTheme"
        @delete="removeTheme"
      />
    </section>

    <section class="space-y-4">
      <div class="flex items-center justify-between gap-4">
        <UiTextView variant="h2" tag="h2">
          {{ $t('theme.event.title') }}
        </UiTextView>

        <UButton
          variant="subtle"
          icon="crpg:plus"
          :label="$t('theme.event.create.title')"
          :disabled="!themes.length"
          @click="openCreateEvent"
        />
      </div>

      <AdminThemeEventsTable
        :events="events"
        :loading="loadingEvents"
        @edit="openEditEvent"
        @delete="removeEvent"
      />
    </section>

    <UModal
      v-model:open="themeModalOpen"
      :title="editingTheme ? $t('theme.edit.title') : $t('theme.create.title')"
    >
      <template #body>
        <AdminThemeForm
          :theme="editingTheme"
          :loading="savingTheme"
          @submit="submitTheme"
        />
      </template>
    </UModal>

    <UModal
      v-model:open="eventModalOpen"
      :title="editingEvent ? $t('theme.event.edit.title') : $t('theme.event.create.title')"
    >
      <template #body>
        <AdminThemeEventForm
          :themes="themes"
          :event="editingEvent"
          :loading="savingEvent"
          @submit="submitEvent"
        />
      </template>
    </UModal>
  </UContainer>
</template>
