<script setup lang="ts">
import type { CalendarOptions } from '@fullcalendar/core'

import dayGridPlugin from '@fullcalendar/daygrid'
import interactionPlugin from '@fullcalendar/interaction'
import timeGridPlugin from '@fullcalendar/timegrid'
import FullCalendar from '@fullcalendar/vue3'
import { useDateFormat, useNow } from '@vueuse/core'

import type { Battle } from '~/models/strategus/battle'

const props = defineProps<{ battles: Battle[] }>()

const calendarRef = useTemplateRef('calendar')

// custom title
const title = ref<string>('')

function updateTitle() {
  const api = calendarRef.value?.getApi()
  if (!api) {
    return
  }

  const start = api.view.currentStart
  const end = api.view.currentEnd

  title.value = formatRange(start, end)
}

onMounted(updateTitle)

function formatRange(start: Date, end: Date) {
  // Пример: Oct 26 – Nov 1, 2025
  const sameMonth = start.getMonth() === end.getMonth()

  const startStr = useDateFormat(start, 'MMM d')

  const endStr = sameMonth
    ? useDateFormat(end, 'd, yyyy')
    : useDateFormat(end, 'MMM d, yyyy')

  return `${startStr} – ${endStr}`
}

function prev() {
  calendarRef.value!.getApi().prev()
  updateTitle()
}

function next() {
  calendarRef.value!.getApi().next()
  updateTitle()
}

function today() {
  calendarRef.value!.getApi().today()
  updateTitle()
}

function changeView(view: 'timeGridDay' | 'timeGridWeek') {
  calendarRef.value?.getApi().changeView(view)
  updateTitle()
}

const calendarOptions = computed(() => ({
  plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
  initialView: 'timeGridWeek',
  headerToolbar: false,
  allDaySlot: false,
  nowIndicator: true,
  expandRows: true,
  height: 'auto',
  contentHeight: 'auto',
  dayMaxEvents: 1,
  eventTimeFormat: {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  },
  slotLabelFormat: {
    hour: '2-digit',
    minute: '2-digit',
    hour12: false,
  },
  events: props.battles
    .filter(b => b.scheduledFor)
    .map(b => ({
      id: String(b.id),
      start: b.scheduledFor!,
      extendedProps: b,
    })),
  // eventClick: (info: any) => {
  //   const id = info.event.id
  // },
} satisfies CalendarOptions))
</script>

<template>
  <div>
    <div class="mb-3 flex items-center justify-between gap-2">
      <UFieldGroup size="xl">
        <UButton icon="crpg:chevron-left" color="neutral" variant="subtle" @click="prev" />
        <UButton color="neutral" variant="subtle" label="Today" @click="today" />
        <UButton icon="crpg:chevron-right" color="neutral" variant="subtle" @click="next" />
      </UFieldGroup>

      <div>
        <UiTextView variant="h3">
          {{ title }}
        </UiTextView>
      </div>

      <UFieldGroup size="xl">
        <UButton color="neutral" variant="subtle" label="Day" @click="changeView('timeGridDay')" />
        <UButton color="neutral" label="Week" variant="subtle" @click="changeView('timeGridWeek')" />
      </UFieldGroup>
    </div>

    <FullCalendar ref="calendar" :options="calendarOptions">
      <template #eventContent="arg">
        <BattleEventCard :arg="arg" />
      </template>
    </FullCalendar>
  </div>
</template>

<style>
:root {
  --fc-small-font-size: .85em;
  --fc-page-bg-color: transparent;
  --fc-neutral-bg-color: rgba(208, 208, 208, 0.3);
  --fc-neutral-text-color: #808080;
  --fc-border-color: var(--border-color-muted);

  --fc-button-text-color: var(--text-color-highlighted);
  --fc-button-bg-color: #2C3E50;
  --fc-button-border-color: #2C3E50;
  --fc-button-hover-bg-color: #1e2b37;
  --fc-button-hover-border-color: #1a252f;
  --fc-button-active-bg-color: #1a252f;
  --fc-button-active-border-color: #151e27;

  --fc-event-bg-color: var(--background-color-accented);
  --fc-event-border-color: var(--border-color-default);
  --fc-event-text-color: #fff;
  --fc-event-selected-overlay-color: rgba(0, 0, 0, 0.25);

  --fc-more-link-bg-color: #d0d0d0;
  --fc-more-link-text-color: inherit;

  --fc-event-resizer-thickness: 8px;
  --fc-event-resizer-dot-total-width: 8px;
  --fc-event-resizer-dot-border-width: 1px;

  --fc-non-business-color: rgba(215, 215, 215, 0.3);
  --fc-bg-event-color: rgb(143, 223, 130);
  --fc-bg-event-opacity: 0.3;
  --fc-highlight-color: rgba(188, 232, 241, 0.3);
  /* --fc-today-bg-color: var(--background-color-muted); */
  --fc-today-bg-color: transparent;

  --fc-now-indicator-color: var(--color-primary);
  --fc-daygrid-event-dot-width: 8px;
}

.fc {
  .fc-button {

  }

  .fc-timegrid-now-indicator-line {
    border-width: 3px 0px 0px;
  }

  .fc-timegrid-slot-minor {
    border-top-style: none;
  }
}
</style>
