<script setup lang="ts">
import type { RouteLocationNormalized } from 'vue-router/auto'

import type { ClanMember } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanApplications } from '~/composables/clan/use-clan-applications'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { usePagination } from '~/composables/use-pagination'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { ClanMemberRole } from '~/models/clan'
import {
  canKickMemberValidate,
  canManageApplicationsValidate,
  canUpdateClanValidate,
  canUpdateMemberValidate,
  getClanMember,
  inviteToClan,
  kickClanMember,
  updateClanMember,
} from '~/services/clan-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  id: string
}>()

definePage({
  meta: {
    bg: 'background-3.webp',
    layout: 'default',
    middleware: 'clanIdParamValidate',
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const userStore = useUserStore()

const { clan, clanId, loadClan } = useClan(props.id)
const { clanMembers, clanMembersCount, isLastMember, loadClanMembers } = useClanMembers()
const { applicationsCount, loadClanApplications } = useClanApplications()

const selfMember = computed(() => getClanMember(clanMembers.value, userStore.user!.id))
const checkIsSelfMember = (member: ClanMember) => member.user.id === selfMember.value?.user.id

const searchModel = ref<string>('')
const filteredClanMembers = computed(() =>
  clanMembers.value.filter(member =>
    member.user.name.toLowerCase().includes(searchModel.value.toLowerCase()),
  ),
)

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
  await inviteToClan(clanId.value, userStore.user!.id)
}

const canUpdateMember = computed(() =>
  selfMember.value === null ? false : canUpdateMemberValidate(selfMember.value.role),
)

const { execute: updateMember } = useAsyncCallback(async (userId: number, selectedRole: ClanMemberRole) => {
  await updateClanMember(clanId.value, userId, selectedRole)
  await Promise.all([loadClanMembers(0, { id: clanId.value })])
  notify(t('clan.member.update.notify.success'))
})

const canKickMember = (member: ClanMember): boolean => {
  if (selfMember.value === null) { return false }
  return canKickMemberValidate(selfMember.value, member, clanMembers.value.length)
}

const { execute: kickMember } = useAsyncCallback(async (member: ClanMember) => {
  await kickClanMember(clanId.value, member.user.id)
  await loadClanMembers(0, { id: clanId.value })

  const isSelfMember = checkIsSelfMember(member)

  if (isSelfMember) {
    // update user clan info
    await userStore.fetchUser()
  }

  notify(
    isSelfMember ? t('clan.member.leave.notify.success') : t('clan.member.kick.notify.success'),
  )
})

const clanMemberDetailModal = ref<boolean>(false)
const selectedCLanMemberId = ref<number | null>(null)

const onOpenMemberDetail = (member: ClanMember) => {
  if (selfMember.value === null || checkIsSelfMember(member)) { return }

  if (canKickMember(member)) {
    selectedCLanMemberId.value = member.user.id
    clanMemberDetailModal.value = true
  }
}

const selectedClanMember = computed(() =>
  clanMembers.value.find(m => m.user.id === selectedCLanMemberId.value),
)

const { pageModel, perPage } = usePagination()

const fetchPageData = async (clanId: number) => {
  await Promise.all([loadClan(0, { id: clanId }), loadClanMembers(0, { id: clanId })])

  if (canManageApplications.value) {
    await loadClanApplications(0, { id: clanId })
  }
}

// TODO: SPEC
onBeforeRouteUpdate(async (to, from) => {
  if (to.name === from.name) {
    await fetchPageData(Number((to as RouteLocationNormalized<'ClansId'>).params.id))
  }
  return true
})

await fetchPageData(clanId.value)
</script>

<template>
  <div
    v-if="clan !== null"
    class="pb-12 pt-24"
  >
    <div class="container mb-8">
      <div class="mb-8 flex justify-center">
        <ClanTagIcon
          :color="clan.primaryColor"
          size="4x"
        />
      </div>

      <Heading
        :title="clan.name"
        data-aq-clan-info="name"
      />
    </div>

    <div class="container">
      <div class="mx-auto mb-10 max-w-lg space-y-6">
        <Divider />

        <div class="flex flex-wrap items-center justify-center gap-4.5">
          <div class="flex items-center gap-1.5">
            <OIcon
              icon="hash"
              size="lg"
              class="text-content-100"
            />
            <span
              class="text-content-200"
              data-aq-clan-info="tag"
            >{{ clan.tag }}</span>
          </div>

          <div class="h-8 w-px select-none bg-border-200" />

          <div class="flex items-center gap-1.5">
            <OIcon
              icon="region"
              size="lg"
              class="text-content-100"
            />
            <div
              class="text-content-200"
              data-aq-clan-info="region"
            >
              {{ $t(`region.${clan.region}`, 0) }}
            </div>
            <Tag
              v-for="l in clan.languages"
              :key="l"
              v-tooltip="$t(`language.${l}`)"
              :label="l"
              variant="primary"
            />
          </div>

          <div class="h-8 w-px select-none bg-border-200" />

          <div class="flex items-center gap-1.5">
            <OIcon
              icon="member"
              size="lg"
              class="text-content-100"
            />
            <span
              class="text-content-200"
              data-aq-clan-info="member-count"
            >
              {{ clanMembersCount }}
            </span>
          </div>
        </div>

        <div
          v-if="clan.description"
          class="mt-7 overflow-x-hidden text-center text-content-400"
          data-aq-clan-info="description"
        >
          {{ clan.description }}
        </div>

        <Divider />
      </div>
    </div>

    <div class="flex items-center justify-center gap-3">
      <OButton
        v-if="canUseClanArmory"
        tag="router-link"
        :to="{ name: 'ClansIdArmory', params: { id: clanId } }"
        variant="primary"
        outlined
        icon-left="armory"
        :label="$t('clan.armory.title')"
        size="xl"
      />

      <template v-if="canManageApplications || canUpdateClan">
        <OButton
          v-if="canManageApplications"
          tag="router-link"
          :to="{ name: 'ClansIdApplications', params: { id: clanId } }"
          variant="primary"
          outlined
          size="xl"
          data-aq-clan-action="clan-application"
        >
          {{ $t('clan.application.title') }}
          <template v-if="applicationsCount !== 0">
            ({{ applicationsCount }})
          </template>
        </OButton>

        <OButton
          v-if="canUpdateClan"
          v-tooltip.bottom="$t('clan.action.settings')"
          tag="router-link"
          :to="{ name: 'ClansIdUpdate', params: { id: clanId } }"
          variant="secondary"
          size="xl"
          outlined
          rounded
          icon-left="settings"
          data-aq-clan-action="clan-update"
        />
      </template>

      <template v-else-if="userStore.clan === null">
        <OButton
          v-if="applicationSent"
          variant="secondary"
          size="xl"
          disabled
          outlined
          data-aq-clan-action="application-sent"
          :label="$t('clan.application.sent')"
        />

        <OButton
          v-else
          variant="primary"
          size="xl"
          :label="$t('clan.application.apply')"
          data-aq-clan-action="apply-to-join"
          @click="apply"
        />
      </template>

      <Modal v-if="!isLastMember && selfMember !== null && canKickMember(selfMember)">
        <OButton
          variant="secondary"
          size="xl"
          outlined
          :label="$t('clan.member.leave.title')"
          data-aq-clan-action="leave-clan"
        />

        <template #popper="{ hide }">
          <div class="space-y-6 px-12 py-11 text-center">
            <h4 class="text-xl">
              {{ $t('clan.member.leave.dialog.title') }}
            </h4>

            <i18n-t
              scope="global"
              keypath="clan.member.leave.dialog.desc"
              tag="p"
            >
              <template #clanName>
                <span class="font-bold">{{ clan.name }}</span>
              </template>
            </i18n-t>

            <div class="flex items-center justify-center gap-4">
              <OButton
                variant="primary"
                outlined
                size="xl"
                :label="$t('action.cancel')"
                data-aq-clan-action="leave-clan-cancel"
                @click="hide"
              />
              <OButton
                variant="primary"
                size="xl"
                :label="$t('action.leave')"
                data-aq-clan-action="leave-clan-confirm"
                @click="
                  () => {
                    selfMember !== null && kickMember(selfMember);
                    hide();
                  }
                "
              />
            </div>
          </div>
        </template>
      </Modal>

      <OButton
        v-if="clan?.discord"
        v-tooltip.bottom="'Discord'"
        variant="secondary"
        size="xl"
        outlined
        rounded
        tag="a"
        icon-left="discord"
        :href="clan?.discord"
        target="_blank"
      />
    </div>

    <div class="container mt-20">
      <div class="mx-auto max-w-3xl">
        <OTable
          v-model:current-page="pageModel"
          :data="filteredClanMembers"
          :per-page="perPage"
          :hoverable="selfMember !== null && selfMember.role !== ClanMemberRole.Member"
          bordered
          :paginated="clanMembers.length > perPage"
          @click="(row) => onOpenMemberDetail(row as ClanMember)"
        >
          <OTableColumn field="user.name">
            <template #header>
              <div class="w-44">
                <OInput
                  v-model="searchModel"
                  type="text"
                  expanded
                  clearable
                  :placeholder="$t('clan.table.column.name')"
                  icon="search"
                  rounded
                  size="xs"
                />
              </div>
            </template>

            <template #default="{ row: member }: { row: ClanMember }">
              <UserMedia
                :class="
                  member.role === ClanMemberRole.Leader ? 'text-more-support' : 'text-content-100'
                "
                :user="member.user"
                :is-self="checkIsSelfMember(member)"
                hidden-clan
              />
            </template>
          </OTableColumn>

          <OTableColumn
            v-slot="{ row: member }: { row: ClanMember }"
            field="role"
            position="right"
            :label="$t('clan.table.column.role')"
            width="100"
          >
            <ClanRole :role="member.role" />
          </OTableColumn>
        </OTable>
      </div>
    </div>

    <Modal
      :shown="clanMemberDetailModal"
      :auto-hide="false"
      data-aq-clan-member-detail-modal
      @apply-hide="selectedCLanMemberId = null"
      @hide="clanMemberDetailModal = false"
    >
      <template #popper="{ hide }">
        <ClanMemberDetail
          v-if="selectedClanMember !== undefined"
          :member="selectedClanMember"
          :can-kick="canKickMember(selectedClanMember)"
          :can-update="canUpdateMember"
          @cancel="hide"
          @kick="
            () => {
              selectedClanMember !== undefined && kickMember(selectedClanMember!);
              hide();
            }
          "
          @update="
            role => {
              selectedClanMember !== undefined && updateMember(selectedClanMember.user.id, role);
              hide();
            }
          "
        />
      </template>
    </Modal>
  </div>
</template>
