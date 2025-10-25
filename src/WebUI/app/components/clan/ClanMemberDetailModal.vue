<script setup lang="ts">
import type { RadioGroupItem } from '@nuxt/ui'

import { LazyAppConfirmActionDialog } from '#components'

import type { ClanMember, ClanMemberRole } from '~/models/clan'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

const { canKick, canUpdate, member } = defineProps<{
  member: ClanMember
  canUpdate: boolean
  canKick: boolean
}>()

const emit = defineEmits<{
  kick: []
  update: [role: ClanMemberRole]
  close: []
}>()

const memberRoleModel = ref<ClanMemberRole>(member.role)

const overlay = useOverlay()
const { t } = useI18n()

const transferRightsConfirmDialog = overlay.create(LazyAppConfirmActionDialog, {
  props: {
    title: t('clan.member.update.confirmationDialog.title'),
    description: t('clan.member.update.confirmationDialog.desc'),
    confirm: member.user.name,
  },
})

async function save() {
  if (memberRoleModel.value === CLAN_MEMBER_ROLE.Leader) {
    if (!(await transferRightsConfirmDialog.open())) {
      return
    }
  }

  emit('update', memberRoleModel.value)
}
</script>

<template>
  <UModal
    :ui="{
      title: 'flex justify-center',
      footer: 'justify-center',
      body: 'space-y-6',
    }"
    @update:open="$emit('close')"
  >
    <template #title>
      <UserMedia :user="member.user" size="xl" hidden-clan />
    </template>

    <template #body>
      <UiCard
        v-if="canUpdate"
        icon="crpg:member"
        :label="$t('clan.roleTitle')"
      >
        <URadioGroup
          v-model="memberRoleModel"
          size="xl"
          :items="Object.values(CLAN_MEMBER_ROLE).map<RadioGroupItem>((role) => ({
            label: `${$t(`clan.role.${role}`)}${role === CLAN_MEMBER_ROLE.Leader ? ` (${$t('clan.member.update.transferRights')})` : ''}`,
            value: role,
          }))"
        />
      </UiCard>

      <i18n-t
        v-if="canKick"
        scope="global"
        keypath="clan.member.kick.title"
        tag="p"
        class="text-center"
      >
        <template #memberLink>
          <AppConfirmActionPopover @confirm="$emit('kick')">
            <ULink
              class="
                cursor-pointer text-error
                hover:text-error/80
              "
            >
              {{ $t('clan.member.kick.memberLink') }}
            </ULink>
          </AppConfirmActionPopover>
        </template>
      </i18n-t>
    </template>

    <template #footer>
      <UButton
        variant="outline"
        size="xl"
        :label="$t('action.cancel')"
        data-aq-clan-member-action="close-detail"
        @click="$emit('close')"
      />

      <UButton
        size="xl"
        :label="$t('action.save')"
        :disabled="member.role === memberRoleModel"
        data-aq-clan-member-action="save"
        @click="save"
      />
    </template>
  </UModal>
</template>
