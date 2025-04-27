<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'
import type { ColumnFiltersState, PaginationState } from '@tanstack/vue-table'

import { getPaginationRowModel } from '@tanstack/vue-table'
import { ClanRole, UInput, UserMedia } from '#components'

import type { ClanMember, ClanMemberRole } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanApplications } from '~/composables/clan/use-clan-applications'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import {
  canKickMemberValidate,
  canManageApplicationsValidate,
  canUpdateClanValidate,
  canUpdateMemberValidate,
} from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  id: string
}>()

definePageMeta({
  props: true,
  layoutOptions: {
    bg: 'background-3.webp',
  },
  middleware: ['clan-id-param-validate'],
  roles: SomeRole,
})

const toast = useToast()
const { t } = useI18n()

const userStore = useUserStore()

const clanId = computed(() => Number(props.id))
const { clan, loadClan } = useClan(clanId)
const {
  clanMembers,
  clanMembersCount,
  isLastMember,
  loadClanMembers,
  loadingClanMembers,
  kickClanMember,
  updateClanMember,
  getClanMember,
} = useClanMembers(clanId)

const {
  applicationsCount,
  loadClanApplications,
  inviteToClan,
} = useClanApplications(clanId)

const selfMember = computed(() => getClanMember(userStore.user!.id))
const checkIsSelfMember = (member: ClanMember) => member.user.id === selfMember.value?.user.id

const searchModel = ref<string>('')

const canManageApplications = computed(() =>
  selfMember.value === null ? false : canManageApplicationsValidate(selfMember.value.role),
)

const canUpdateClan = computed(() =>
  selfMember.value === null ? false : canUpdateClanValidate(selfMember.value.role),
)

const canUseClanArmory = computed(() => Boolean(selfMember.value))

const applicationSent = ref<boolean>(false)
const apply = async () => {
  applicationSent.value = true
  await inviteToClan(userStore.user!.id)
}

const canUpdateMember = computed(() =>
  selfMember.value === null ? false : canUpdateMemberValidate(selfMember.value.role),
)

const { execute: updateMember } = useAsyncCallback(async (userId: number, selectedRole: ClanMemberRole) => {
  await updateClanMember(userId, selectedRole)
  await loadClanMembers()
  toast.add({
    color: 'success',
    close: false,
    title: t('clan.member.update.notify.success'),
  })
})

const canKickMember = (member: ClanMember): boolean => {
  if (selfMember.value === null) {
    return false
  }
  return canKickMemberValidate(selfMember.value, member, clanMembers.value.length)
}

const { execute: kickMember } = useAsyncCallback(async (member: ClanMember) => {
  await kickClanMember(member.user.id)
  await loadClanMembers()

  const isSelfMember = checkIsSelfMember(member)

  if (isSelfMember) {
    await userStore.fetchUser() // update user clan info
  }

  toast.add({
    title: isSelfMember ? t('clan.member.leave.notify.success') : t('clan.member.kick.notify.success'),
    color: 'success',
    close: false,
  })
})

const selectedClanMemberId = ref<number | null>(null)
const onOpenMemberDetail = (member: ClanMember) => {
  if (!selfMember.value || checkIsSelfMember(member)) {
    return
  }

  if (canKickMember(member)) {
    selectedClanMemberId.value = member.user.id
  }
}

const selectedClanMember = computed(() => clanMembers.value.find(m => m.user.id === selectedClanMemberId.value))

const fetchPageData = async () => {
  await Promise.all([
    loadClan(),
    loadClanMembers(),
  ])

  if (canManageApplications.value) {
    await loadClanApplications()
  }
}

// TODO: SPEC
// onBeforeRouteUpdate(async (to, from) => {
//   if (to.name === from.name) {
//     await fetchPageData(Number((to as RouteLocationNormalized<'ClansId'>).params.id))
//   }
//   return true
// })

fetchPageData()

const table = useTemplateRef('table')

const pagination = ref<PaginationState>(getInitialPaginationState())

// TODO: to compose
function getInitialPaginationState(): PaginationState {
  return {
    pageIndex: 0,
    pageSize: 10, // TODO: FIXME:
  }
}

const columnFilters = ref<ColumnFiltersState>([
])

const columns: TableColumn<ClanMember>[] = [
  {
    accessorKey: 'user.name',
    id: 'user_name',
    // @ts-expect-error TODO:
    header: ({ column }) => h(UInput, {
      'icon': 'crpg:search',
      'variant': 'ghost',
      'size': 'xs',
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
        td: tw`w-36`,
      },
    },
  },
]
</script>

<template>
  <div
    v-if="clan !== null"
    class="pt-24 pb-12"
  >
    <UContainer class="mb-8">
      <div class="mb-5 flex justify-center">
        <ClanTagIcon
          :color="clan.primaryColor"
          class="size-12"
        />
      </div>

      <UiHeading
        :title="clan.name"
        class="mb-14"
        data-aq-clan-info="name"
      />

      <div class="mx-auto mb-10 max-w-lg space-y-6">
        <UiDecorSeparator />

        <div class="flex flex-wrap items-center justify-center gap-4.5">
          <UiDataCell>
            <template #leftContent>
              <UIcon name="crpg:hash" class="size-6" />
            </template>
            <!-- TODO: text view cmp? -->
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
                    size="sm"
                    color="primary"
                    variant="soft"
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
        </div>

        <div
          v-if="clan.description"
          class="mt-7 overflow-x-hidden text-center text-content-400"
          data-aq-clan-info="description"
        >
          {{ clan.description }}
        </div>

        <UiDecorSeparator />
      </div>

      <div class="mb-12 flex items-center justify-center gap-3">
        <UButton
          v-if="canUseClanArmory"
          color="primary"
          variant="outline"
          icon="crpg:armory"
          :label="$t('clan.armory.title')"
          size="xl"
          :to="{ name: 'clans-id-armory', params: { id: clanId } }"
        />

        <template v-if="canManageApplications || canUpdateClan">
          <UButton
            v-if="canManageApplications"
            :to="{ name: 'clans-id-applications', params: { id: clanId } }"
            color="primary"
            variant="outline"
            size="xl"
            data-aq-clan-action="clan-application"
            :label="`${$t('clan.application.title')}${applicationsCount ? ` (${applicationsCount})` : ''}`"
          />
          <UButton
            v-if="canUpdateClan"
            :to="{ name: 'clans-id-update', params: { id: clanId } }"
            color="secondary"
            variant="outline"
            size="xl"
            icon="crpg:settings"
            data-aq-clan-action="clan-update"
          />
        </template>

        <template v-else-if="!userStore.clan">
          <UButton
            v-if="applicationSent"
            color="secondary"
            variant="outline"
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

        <UModal
          v-if="!isLastMember && selfMember && canKickMember(selfMember)"
          :title="$t('clan.member.leave.dialog.title')"
          :ui="{ footer: 'justify-center' }"
          :close="{
            size: 'sm',
            color: 'secondary',
            variant: 'solid',
          }"
        >
          <UButton
            color="secondary"
            variant="outline"
            size="xl"
            :label="$t('clan.member.leave.title')"
            data-aq-clan-action="leave-clan"
          />

          <template #body>
            <div class="space-y-6 text-center">
              <i18n-t
                scope="global"
                keypath="clan.member.leave.dialog.desc"
                tag="p"
              >
                <template #clanName>
                  <span class="inline-flex items-center gap-0.5 align-middle">
                    <ClanTagIcon :color="clan.primaryColor" /> {{ clan.name }}
                  </span>
                </template>
              </i18n-t>
            </div>
          </template>
          <template #footer="{ close }">
            <UButton
              color="primary"
              variant="outline"
              size="xl"
              :label="$t('action.cancel')"
              data-aq-clan-action="leave-clan-cancel"
              @click="close"
            />
            <UButton
              color="primary"
              size="xl"
              :label="$t('action.leave')"
              data-aq-clan-action="leave-clan-confirm"
              @click="() => {
                selfMember && kickMember(selfMember);
                close();
              }"
            />
          </template>
        </UModal>

        <UButton
          v-if="clan.discord"
          color="secondary"
          variant="outline"
          size="xl"
          icon="crpg:discord"
          target="_blank"
          :href="clan.discord"
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
          @select="(row) => onOpenMemberDetail(row.original)"
        >
          <template #empty>
            <UiResultNotFound />
          </template>
        </UTable>

        <UPagination
          v-if="table?.tableApi.getCanNextPage() || table?.tableApi.getCanPreviousPage()"
          class="flex justify-center"
          variant="soft"
          color="secondary"
          active-variant="solid"
          active-color="primary"
          :page="pagination.pageIndex + 1"
          :show-controls="false"
          :default-page="(table?.tableApi.initialState.pagination.pageIndex || 0) + 1"
          :items-per-page="table?.tableApi.initialState.pagination.pageSize"
          :total="table?.tableApi.getFilteredRowModel().rows.length"
          @update:page="(p) => table?.tableApi.setPageIndex(p - 1)"
        />
      </div>
    </UContainer>

    <ClanMemberDetailModal
      v-if="selectedClanMember"
      open
      data-aq-clan-member-detail-modal
      :member="selectedClanMember"
      :can-kick="canKickMember(selectedClanMember)"
      :can-update="canUpdateMember"
      @kick="kickMember(selectedClanMember!)"
      @update="role => {
        updateMember(selectedClanMember!.user.id, role)
        selectedClanMemberId = null
      }"
      @cancel="() => (selectedClanMemberId = null)"
    />
  </div>
</template>
