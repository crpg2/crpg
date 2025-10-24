<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState } from '@tanstack/vue-table'

import { getPaginationRowModel } from '@tanstack/vue-table'
import { ClanRole, LazyClanActionLeaveConfirmDialog, LazyClanMemberDetailModal, UInput, UserMedia } from '#components'

import type { ClanMember, ClanMemberRole } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanApplications } from '~/composables/clan/use-clan-applications'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import {
  canKickMemberValidate,
  canManageApplicationsValidate,
  canUpdateClanValidate,
  canUpdateMemberValidate,
} from '~/services/clan-service'

definePageMeta({
  layoutOptions: {
    bg: 'background-3.webp',
  },
  roles: SomeRole,
})

const { t } = useI18n()
const overlay = useOverlay()

const { user, fetchUser, clan: userClan } = useUser()
const { clan } = useClan()
const {
  clanMembers,
  clanMembersCount,
  isLastMember,
  loadClanMembers,
  loadingClanMembers,
  kickClanMember,
  updateClanMember,
  getClanMember,
} = useClanMembers()

const selfMember = computed(() => getClanMember(user.value!.id))

const {
  applicationsCount,
  inviteToClan,
  loadClanApplications,
} = useClanApplications(false)

watchEffect(() => {
  if (selfMember.value) {
    loadClanApplications()
  }
})

const canUpdateClan = computed(() => Boolean(selfMember.value && canUpdateClanValidate(selfMember.value.role)))
const canManageApplications = computed(() => Boolean(selfMember.value && canManageApplicationsValidate(selfMember.value.role)))

const canKickMember = (member: ClanMember) => Boolean(selfMember.value && canKickMemberValidate(selfMember.value, member, clanMembers.value.length))
const checkIsSelfMember = (member: ClanMember) => member.user.id === selfMember.value?.user.id
const canUseClanArmory = computed(() => Boolean(selfMember.value))
const canLeave = computed(() => Boolean(selfMember.value && canKickMember(selfMember.value)) && !isLastMember.value)

const applicationSent = ref<boolean>(false)

function apply() {
  applicationSent.value = true
  inviteToClan(user.value!.id)
}

const [onLeaveClan] = useAsyncCallback(async () => {
  if (!selfMember.value) {
    return
  }

  await kickClanMember(selfMember.value.user.id)
  await Promise.all([
    loadClanMembers(),
    fetchUser(), // update user clan info
  ])
}, {
  successMessage: t('clan.member.leave.notify.success'),
  pageLoading: true,
})

const clanLeaveConfirmDialog = overlay.create(LazyClanActionLeaveConfirmDialog, {
  props: {
    clan: clan.value,
  },
})

async function leaveClan() {
  if (!(await clanLeaveConfirmDialog.open())) {
    return
  }

  onLeaveClan()
}

const [updateMember] = useAsyncCallback(async (userId: number, selectedRole: ClanMemberRole) => {
  await updateClanMember(userId, selectedRole)
  await loadClanMembers()
}, {
  successMessage: t('clan.member.update.notify.success'),
  pageLoading: true,
})

const [kickMember] = useAsyncCallback(async (member: ClanMember) => {
  await kickClanMember(member.user.id)
  await loadClanMembers()
}, {
  successMessage: t('clan.member.kick.notify.success'),
  pageLoading: true,
})

const memberDetailModal = overlay.create(LazyClanMemberDetailModal)

const openMemberDetail = (member: ClanMember) => {
  if (!selfMember.value || checkIsSelfMember(member)) {
    return
  }

  const canKick = canKickMember(member)

  if (!canKick) {
    return
  }

  const canUpdate = canUpdateMemberValidate(selfMember.value.role)

  memberDetailModal.open({
    member,
    canKick,
    canUpdate,
    onKick: () => {
      kickMember(member)
      memberDetailModal.close()
    },
    onUpdate: (role) => {
      updateMember(member.user.id, role)
      memberDetailModal.close()
    },
  })
}

const table = useTemplateRef('table')
const { getInitialPaginationState, pagination } = usePagination()
const searchModel = ref<string | undefined>(undefined)
const columnFilters = ref<ColumnFiltersState>([])
const columns: TableColumn<ClanMember>[] = [
  {
    accessorKey: 'user.name',
    id: 'user_name',
    // @ts-expect-error TODO:
    header: ({ column }) => h(UInput, {
      'icon': 'crpg:search',
      'placeholder': t('clan.table.column.name'),
      'modelValue': column.getFilterValue() as string,
      'onUpdate:modelValue': column.setFilterValue,
    }),
    cell: ({ row }) => h(UserMedia, {
      user: row.original.user,
      isSelf: checkIsSelfMember(row.original),
      hiddenClan: true,
    }),
  },
  {
    accessorFn: row => row.role,
    header: t('clan.table.column.role'),
    cell: ({ row }) => h(ClanRole, { role: row.original.role }),
    meta: {
      class: {
        th: tw`w-36 text-right`,
        td: tw`w-36 text-right`,
      },
    },
  },
]
</script>

<template>
  <UContainer
    v-if="clan !== null"
    class="space-y-8 pt-24 pb-12"
  >
    <UiHeading
      :title="clan.name"
      data-aq-clan-info="name"
    >
      <template #icon>
        <ClanTagIcon
          :color="clan.primaryColor"
          class="size-12"
        />
      </template>
    </UiHeading>

    <div class="mx-auto max-w-lg space-y-5">
      <UiDecorSeparator />

      <div class="flex flex-wrap items-center justify-center gap-4.5">
        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:hash" class="size-6" />
          </template>
          <span data-aq-clan-info="tag">{{ clan.tag }}</span>
        </UiDataCell>

        <USeparator orientation="vertical" class="h-8" />

        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:region" class="size-6" />
          </template>
          <span data-aq-clan-info="region"> {{ $t(`region.${clan.region}`, 0) }}</span>
          <template #rightContent>
            <div class="flex items-center gap-1">
              <UTooltip
                v-for="l in clan.languages"
                :key="l"
                :text="$t(`language.${l}`)"
              >
                <UBadge
                  :label="l"
                  color="primary"
                  variant="subtle"
                />
              </UTooltip>
            </div>
          </template>
        </UiDataCell>

        <USeparator orientation="vertical" class="h-8" />

        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:member" class="size-6" />
          </template>
          <span data-aq-clan-info="member-count">{{ clanMembersCount }}</span>
        </UiDataCell>

        <template v-if="clan.discord">
          <USeparator orientation="vertical" class="h-8" />

          <UButton
            variant="outline"
            icon="crpg:discord"
            target="_blank"
            :href="clan.discord"
          />
        </template>
      </div>

      <UiTextView
        v-if="clan.description"
        variant="p"
        class="mt-7 text-center"
        data-aq-clan-info="description"
      >
        {{ clan.description }}
      </UiTextView>

      <UiDecorSeparator />
    </div>

    <div class="flex items-center justify-center gap-3">
      <UButton
        v-if="canUseClanArmory"
        color="primary"
        variant="outline"
        icon="crpg:armory"
        :label="$t('clan.armory.title')"
        size="xl"
        :to="{ name: 'clans-id-armory', params: { id: clan.id } }"
      />

      <template v-if="canManageApplications || canUpdateClan">
        <UChip
          v-if="canManageApplications"
          :show="Boolean(applicationsCount)"
          inset
          size="2xl"
          :ui="{ base: 'bg-[var(--color-notification)]' }"
        >
          <UButton
            :to="{ name: 'clans-id-applications', params: { id: clan.id } }"
            color="neutral"
            variant="outline"
            size="xl"
            data-aq-clan-action="clan-application"
            :label="`${$t('clan.application.title')}${applicationsCount ? ` (${applicationsCount})` : ''}`"
          />
        </UChip>

        <UButton
          v-if="canUpdateClan"
          :to="{ name: 'clans-id-update', params: { id: clan.id } }"
          color="neutral"
          variant="outline"
          size="xl"
          icon="crpg:settings"
          data-aq-clan-action="clan-update"
        />
      </template>

      <template v-else-if="!userClan">
        <UButton
          v-if="applicationSent"
          color="success"
          variant="soft"
          size="xl"
          disabled
          data-aq-clan-action="application-sent"
          :label="$t('clan.application.sent')"
        />
        <UButton
          v-else
          color="primary"
          size="xl"
          :label="$t('clan.application.apply')"
          data-aq-clan-action="apply-to-join"
          @click="apply"
        />
      </template>

      <UButton
        v-if="canLeave"
        color="neutral"
        size="xl"
        variant="outline"
        icon="crpg:logout"
        :label="$t('clan.member.leave.title')"
        data-aq-clan-action="leave-clan"
        @click="leaveClan"
      />
    </div>

    <div class="mx-auto max-w-3xl space-y-4">
      <UTable
        ref="table"
        v-model:pagination="pagination"
        v-model:column-filters="columnFilters"
        v-model:global-filter="searchModel"
        class="relative rounded-md border border-muted"
        :loading="loadingClanMembers"
        :data="clanMembers"
        :columns
        :initial-state="{
          pagination: getInitialPaginationState(),
        }"
        :pagination-options="{
          getPaginationRowModel: getPaginationRowModel(),
        }"
        @select="(_, row) => openMemberDetail(row.original)"
      >
        <template #empty>
          <UiResultNotFound />
        </template>
      </UTable>

      <UiGridPagination v-if="table?.tableApi" :table-api="toRef(() => table!.tableApi)" />
    </div>
  </UContainer>
</template>
