<script setup lang="ts">
import type { ZonedDateTime } from '@internationalized/date'

import { DateFormatter, getLocalTimeZone, now, parseZonedDateTime } from '@internationalized/date'

import { useModerationUser } from '~/composables/moderator/use-moderation-user'
import { usePageLoading } from '~/composables/utils/use-page-loading'
import { Sort, useSort } from '~/composables/utils/use-sort'
import { ActivityLogType } from '~/models/activity-logs'
import { getActivityLogs } from '~/services/activity-logs-service'

const dateFormatter = new DateFormatter('en-US', {
  dateStyle: 'medium',
})

const { moderationUser } = useModerationUser()

const { t } = useI18n()

const from = useRouteQuery(
  'from',
  now(getLocalTimeZone()).subtract({ minutes: 5 }).toString(), // Show logs for the last 5 minutes by default
  {
    transform: {
      set: (value: ZonedDateTime) => value.toString(),
      get: value => parseZonedDateTime(value, 'compatible'),
    },
  },
)

const to = useRouteQuery(
  'to',
  now(getLocalTimeZone()).toString(),
  {
    transform: {
      set: (value: ZonedDateTime) => value.toString(),
      get: value => parseZonedDateTime(value, 'compatible'),
    },
  },
)

const types = useRouteQuery<ActivityLogType[]>('types', [])

const addType = (type: ActivityLogType) => {
  types.value = [...new Set([...types.value]), type]
}

const additionalUsers = useRouteQuery('additionalUsers', [], { transform: value => value.map(Number) })

const toggleAdditionalUser = (userId: number) => {
  additionalUsers.value = toggle(additionalUsers.value, userId)
}

const { sort, toggleSort } = useSort('createdAt')

const {
  state: activityLogs,
  execute: fetchActivityLogs,
  isLoading: isLoadingActivityLogs,
} = useAsyncState(
  () => getActivityLogs({
    from: from.value.toDate(),
    to: to.value.toDate(),
    types: types.value,
    userIds: [moderationUser.value.id, ...additionalUsers.value.map(Number)],
  }),
  {
    activityLogs: [],
    dict: {
      users: [],
      characters: [],
      clans: [],
    },
  },
  {
    immediate: true,
    resetOnExecute: false,
    throwError: true,
  },
)

const sortedActivityLogs = computed(() => activityLogs.value.activityLogs.toSorted((a, b) =>
  sort.value === Sort.ASC
    ? a.createdAt.getTime() - b.createdAt.getTime()
    : b.createdAt.getTime() - a.createdAt.getTime(),
))

watch([types, to, from, additionalUsers], () => {
  fetchActivityLogs()
})

const { togglePageLoading } = usePageLoading()

watchEffect(() => {
  togglePageLoading(isLoadingActivityLogs.value)
})
</script>

<template>
  <div class="mx-auto max-w-3xl space-y-4 pb-8">
    <div class="flex justify-between gap-4">
      <UButtonGroup>
        <USelectMenu
          v-model="types"
          class="max-w-44"
          color="neutral"
          variant="subtle"
          :placeholder="$t('activityLog.form.type')"
          multiple
          :items="Object.values(ActivityLogType)"
          :ui="{
            content: 'w-auto',
          }"
        />

        <!-- TODO: to datepicker cpm -->
        <UPopover>
          <UButton
            color="neutral"
            variant="subtle"
            icon="crpg:calendar"
          >
            {{ dateFormatter.format(from.toDate()) }}
          </UButton>
          <template #content>
            <UCalendar v-model="from" :max-value="to" />
          </template>
        </UPopover>

        <UPopover>
          <UButton
            color="neutral"
            variant="subtle"
            icon="crpg:calendar"
          >
            {{ dateFormatter.format(to.toDate()) }}
          </UButton>
          <template #content>
            <UCalendar v-model="to" :min-value="from" />
          </template>
        </UPopover>
      </UButtonGroup>

      <UButton
        :icon="sort === Sort.ASC ? 'crpg:chevron-up' : 'crpg:chevron-down'"
        color="neutral"
        variant="subtle"
        :label="$t('activityLog.sort.createdAt')"
        data-aq-activityLogs-sort-btn
        @click="toggleSort"
      />
    </div>

    <div class="flex flex-wrap items-center gap-4">
      <div
        v-for="additionalUserId in additionalUsers"
        :key="additionalUserId"
        class="flex items-center gap-1"
        data-aq-activityLogs-additionalUser
      >
        <UButton
          size="xs"
          icon="crpg:close"
          variant="ghost"
          color="neutral"
          data-aq-activityLogs-additionalUser-remove
          @click="toggleAdditionalUser(Number(additionalUserId))"
        />
        <NuxtLink :to="{ name: 'moderator-user-id-restrictions', params: { id: additionalUserId } }">
          <UserMedia
            v-if="activityLogs.dict.users.find(user => user.id === additionalUserId)"
            :user="activityLogs.dict.users.find(user => user.id === additionalUserId)!"
          />
        </NuxtLink>
      </div>

      <UModal
        :close="{
          size: 'sm',
          color: 'secondary',
          variant: 'solid',
        }"
        :title="t('findUser.title')"
        :ui="{
          content: 'min-w-[720px]',
        }"
      >
        <UButton
          icon="crpg:plus"
          color="neutral"
          variant="subtle"
          :label="$t('activityLog.form.addUser')"
        />

        <template #body="{ close }">
          <ModeratorUserFinder>
            <template #user-prepend="userData">
              <UButton
                size="xs"
                icon="crpg:plus"
                color="neutral"
                variant="subtle"
                :label="$t('activityLog.form.addUser')"
                data-aq-activityLogs-userFinder-addUser-btn
                @click="() => {
                  toggleAdditionalUser(userData.id);
                  close();
                } "
              />
            </template>
          </ModeratorUserFinder>
        </template>
      </UModal>
    </div>

    <div class="flex flex-col flex-wrap gap-4">
      <ModeratorActivityLogItem
        v-for="activityLog in sortedActivityLogs"
        :key="activityLog.id"
        :activity-log="activityLog"
        :is-self-user="activityLog.userId === moderationUser.id"
        :user="activityLog.userId === moderationUser.id
          ? moderationUser
          : activityLogs.dict.users.find(user => user.id === activityLog.userId)!"
        :dict="activityLogs.dict"
        @add-user="toggleAdditionalUser"
        @add-type="addType"
      />
    </div>
  </div>
</template>
