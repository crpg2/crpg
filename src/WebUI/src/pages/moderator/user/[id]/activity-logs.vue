<script setup lang="ts">
import { Sort, useSort } from '~/composables/use-sort'
import { ActivityLogType } from '~/models/activity-logs'
import { getActivityLogs } from '~/services/activity-logs-service'
import { moderationUserKey } from '~/symbols/moderator'

const props = defineProps<{ id: string }>()

definePage({
  meta: {
    roles: ['Moderator', 'Admin'],
  },
  props: true,
})

const user = injectStrict(moderationUserKey)
const router = useRouter()
const route = useRoute()

const from = computed({
  get() {
    if (route.query?.from === undefined) {
      const fromDate = new Date()
      fromDate.setMinutes(fromDate.getMinutes() - 5) // Show logs for the last 5 minutes by default
      return fromDate
    }

    return new Date(route.query.from as string)
  },

  set(val: Date) {
    router.push({
      query: {
        ...route.query,
        from: val.toISOString(),
      },
    })
  },
})

const to = computed({
  get() {
    return route.query?.to ? new Date(route.query.to as string) : new Date()
  },

  set(val: Date) {
    router.push({
      query: {
        ...route.query,
        to: String(val.toISOString()),
      },
    })
  },
})

const types = computed({
  get() {
    return (route.query?.types as ActivityLogType[]) || []
  },

  set(val: ActivityLogType[]) {
    router.push({
      query: {
        ...route.query,
        types: val,
      },
    })
  },
})

const addType = (type: ActivityLogType) => {
  types.value = [...new Set([...((route.query?.types as ActivityLogType[]) || []), type])]
}

const additionalUsers = computed({
  get() {
    return ((route.query?.additionalUsers as string[]) || []).map(Number)
  },

  async set(val: number[]) {
    await router.push({
      query: {
        ...route.query,
        additionalUsers: val,
      },
    })

    fetchActivityLogs()
  },
})

const addAdditionalUser = (id: number) => {
  additionalUsers.value = [...new Set([...additionalUsers.value, id])]
}

const removeAdditionalUser = (userId: number) => {
  additionalUsers.value = additionalUsers.value.filter(id => id !== userId)
}

const { sort, toggleSort } = useSort('createdAt')

const {
  state: activityLogs,
  execute: fetchActivityLogs,
  isLoading: isLoadingActivityLogs,
} = useAsyncState(
  () =>
    getActivityLogs({
      from: from.value,
      to: to.value,
      type: types.value,
      userId: [Number(props.id), ...additionalUsers.value.map(Number)],
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
    immediate: false,
    resetOnExecute: false,
  },
)

const sortedActivityLogs = computed(() =>
  [...activityLogs.value.activityLogs].sort((a, b) =>
    sort.value === Sort.ASC
      ? a.createdAt.getTime() - b.createdAt.getTime()
      : b.createdAt.getTime() - a.createdAt.getTime(),
  ),
)

fetchActivityLogs()
</script>

<template>
  <div class="mx-auto max-w-3xl space-y-8 pb-8">
    <OField class="w-full">
      <OField :label="$t('activityLog.form.type')">
        <VDropdown :triggers="['click']">
          <template #default="{ shown }">
            <OInput
              class="w-44 cursor-pointer overflow-x-hidden text-ellipsis"
              :model-value="types.join(',')"
              type="text"
              size="sm"
              expanded
              :placeholder="$t('activityLog.form.type')"
              :icon="shown ? 'chevron-up' : 'chevron-down'"
              :icon-right="types.length !== 0 ? 'close' : undefined"
              icon-right-clickable
              readonly
              @icon-right-click.stop="types = []"
            />
          </template>
          <template #popper>
            <div class="max-h-60 min-w-60 max-w-xs overflow-y-auto">
              <DropdownItem
                v-for="activityLogType in Object.keys(ActivityLogType)"
                :key="activityLogType"
              >
                <OCheckbox
                  v-model="types"
                  :native-value="activityLogType"
                >
                  {{ activityLogType }}
                </OCheckbox>
              </DropdownItem>
            </div>
          </template>
        </VDropdown>
      </OField>

      <OField :label="$t('activityLog.form.from')">
        <ODateTimePicker
          v-model="from"
          size="sm"
          locale="en"
          clearable
          expanded
          icon-right="calendar"
          datepicker-wrapper-class="w-44"
        />
      </OField>

      <OField :label="$t('activityLog.form.to')">
        <ODateTimePicker
          v-model="to"
          size="sm"
          locale="en"
          expanded
          :max="new Date()"
          icon-right="calendar"
          datepicker-wrapper-class="w-44"
        />
      </OField>

      <div class="flex items-end gap-4">
        <OButton
          size="sm"
          icon-left="search"
          :label="$t('action.search')"
          expanded
          variant="primary"
          :loading="isLoadingActivityLogs"
          @click="fetchActivityLogs"
        />
      </div>
    </OField>

    <div class="flex flex-wrap items-center gap-4">
      <div
        v-for="additionalUserId in additionalUsers"
        :key="additionalUserId"
        class="flex items-center gap-1"
        data-aq-activityLogs-additionalUser
      >
        <OButton
          size="2xs"
          icon-left="close"
          rounded
          variant="transparent"
          data-aq-activityLogs-additionalUser-remove
          @click="removeAdditionalUser(Number(additionalUserId))"
        />

        <RouterLink
          :to="{ name: 'ModeratorUserIdRestrictions', params: { id: additionalUserId } }"
          class="inline-block hover:text-content-100"
        >
          <UserMedia
            v-if="activityLogs.dict.users.find(user => user.id === additionalUserId)"
            :user="activityLogs.dict.users.find(user => user.id === additionalUserId)!"
          />
        </RouterLink>
      </div>

      <Modal
        closable
        :auto-hide="false"
        class="self-end"
      >
        <OButton
          size="xs"
          icon-left="add"
          :label="$t('activityLog.form.addUser')"
          variant="secondary"
        />

        <template #popper="{ hide }">
          <div class="min-w-[720px] space-y-6 px-12 py-8">
            <div class="pb-4 text-center text-xl text-content-100">
              {{ $t('findUser.title') }}
            </div>
            <UserFinder>
              <template #user-prepend="userData">
                <OButton
                  size="2xs"
                  icon-left="add"
                  :label="$t('activityLog.form.addUser')"
                  variant="secondary"
                  data-aq-activityLogs-userFinder-addUser-btn
                  @click=" {
                    addAdditionalUser(userData.id);
                    hide();
                  }
                  "
                />
              </template>
            </UserFinder>
          </div>
        </template>
      </Modal>

      <div class="ml-auto mr-0">
        <OButton
          v-tooltip="sort === Sort.ASC ? $t('sort.directions.asc') : $t('sort.directions.desc')"
          size="xs"
          :icon-right="sort === Sort.ASC ? 'chevron-up' : 'chevron-down'"
          variant="secondary"
          :label="$t('activityLog.sort.createdAt')"
          data-aq-activityLogs-sort-btn
          @click="toggleSort"
        />
      </div>
    </div>

    <OLoading
      v-if="isLoadingActivityLogs" :full-page="false"
      active
      icon-size="xl"
    />

    <div
      v-else
      class="flex flex-col flex-wrap gap-4"
    >
      <ActivityLogItem
        v-for="activityLog in sortedActivityLogs"
        :key="activityLog.id"
        :activity-log="activityLog"
        :is-self-user="activityLog.userId === user!.id"
        :user="
          activityLog.userId === user!.id
            ? user!
            : activityLogs.dict.users.find(user => user.id === activityLog.userId)!
        "
        :dict="activityLogs.dict"
        @add-user="addAdditionalUser"
        @add-type="addType"
      />
    </div>
  </div>
</template>
