<script setup lang="ts">
import type { ModalSlots } from '@nuxt/ui' // or '#ui/types'

import type { ClanMember } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useClanApplications } from '~/composables/clan/use-clan-applications'
import { useClanMembers } from '~/composables/clan/use-clan-members'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { usePagination } from '~/composables/utils/use-pagination'
import { ClanMemberRole } from '~/models/clan'
import { SomeRole } from '~/models/role'
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

const { $notify } = useNuxtApp()
const { t } = useI18n()

const userStore = useUserStore()

const clanId = computed(() => Number(props.id))
const { clan, loadClan } = useClan()
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
  $notify(t('clan.member.update.notify.success'))
})

const canKickMember = (member: ClanMember): boolean => {
  if (selfMember.value === null) {
    return false
  }
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

  $notify(isSelfMember
    ? t('clan.member.leave.notify.success')
    : t('clan.member.kick.notify.success'),
  )
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

const { pageModel, perPage } = usePagination()

const fetchPageData = async (clanId: number) => {
  await Promise.all([
    loadClan(0, { id: clanId }),
    loadClanMembers(0, { id: clanId }),
  ])

  if (canManageApplications.value) {
    await loadClanApplications(0, { id: clanId })
  }
}

// TODO: SPEC
// onBeforeRouteUpdate(async (to, from) => {
//   if (to.name === from.name) {
//     await fetchPageData(Number((to as RouteLocationNormalized<'ClansId'>).params.id))
//   }
//   return true
// })

fetchPageData(clanId.value)
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
        data-aq-clan-info="name"
      />
    </UContainer>

    <UContainer>
      <div class="mx-auto mb-10 max-w-lg space-y-6">
        <!-- TODO: -->
        <UiDivider />

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

        <!-- TODO: -->
        <UiDivider />
      </div>
    </UContainer>

    <div class="flex items-center justify-center gap-3">
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
        <template #body="{ close }">
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

    <div class="container mt-20">
      <div class="mx-auto max-w-3xl">
        <OTable
          v-model:current-page="pageModel"
          :data="filteredClanMembers"
          :per-page="perPage"
          :hoverable="selfMember?.role !== ClanMemberRole.Member"
          bordered
          :paginated="clanMembers.length > perPage"
          @click="onOpenMemberDetail"
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

    <!-- @apply-hide="selectedCLanMemberId = null"
      @hide="clanMemberDetailModal = false" -->
    <ClanMemberDetailModal
      v-if="selectedClanMember"
      open
      data-aq-clan-member-detail-modal
      :member="selectedClanMember"
      :can-kick="canKickMember(selectedClanMember)"
      :can-update="canUpdateMember"
      @kick="() => {
        kickMember(selectedClanMember!);
      }"
      @update="role => {
        updateMember(selectedClanMember!.user.id, role);
      }"
      @cancel="() => {
        selectedClanMemberId = null
      }"
    />
    <!-- <UModal
      :open="Boolean(selectedClanMember)"
      title="dd"
      :auto-hide="false"
      data-aq-clan-member-detail-modal
    >
      <template #content="{ close }">
        <ClanMemberDetailModal
          v-if="selectedClanMember"
          :member="selectedClanMember"
          :can-kick="canKickMember(selectedClanMember)"
          :can-update="canUpdateMember"
          @cancel="close"
          @kick="() => {
            kickMember(selectedClanMember!);
            close();
          }"
          @update="role => {
            updateMember(selectedClanMember!.user.id, role);
            close();
          }"
        />
      </template>
    </UModal> -->
  </div>
</template>
