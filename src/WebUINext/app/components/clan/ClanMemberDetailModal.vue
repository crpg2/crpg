<script setup lang="ts">
import type { RadioGroupItem } from '@nuxt/ui'

import type { ClanMember, ClanMemberRole } from '~/models/clan'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

const props = defineProps<{
  member: ClanMember
  canUpdate: boolean
  canKick: boolean
  open: boolean
}>()

const emit = defineEmits<{
  kick: []
  update: [role: ClanMemberRole]
  cancel: []
}>()

const memberRoleModel = ref<ClanMemberRole>(props.member.role)

const [shownConfirmTransferDialog, toggleConfirmTransferDialog] = useToggle()

const onSave = () => {
  if (memberRoleModel.value === CLAN_MEMBER_ROLE.Leader) {
    toggleConfirmTransferDialog(true)
    return
  }

  if (memberRoleModel.value !== props.member.role) {
    emit('update', memberRoleModel.value)
    return
  }

  emit('cancel')
}
</script>

<template>
  <div>
    <UModal
      :open
      :ui="{ footer: 'justify-center', body: 'space-y-6' }"
      :close="{
        size: 'sm',
        color: 'secondary',
        variant: 'solid',
      }"
      @update:open="$emit('cancel')"
    >
      <template #title>
        <UserMedia :user="member.user" size="xl" />
      </template>

      <template #body>
        <UCard
          v-if="canUpdate"
          icon="member"
          :label="$t('clan.roleTitle')"
        >
          <template #header>
            <UiDataCell>
              <template #leftContent>
                <UIcon name="crpg:member" class="size-6" />
              </template>
              <div class="text-sm">
                {{ $t('clan.roleTitle') }}
              </div>
            </UiDataCell>
          </template>

          <URadioGroup
            v-model="memberRoleModel"
            :items="Object.values(CLAN_MEMBER_ROLE).map<RadioGroupItem>((role) => ({
              label: `
                ${$t(`clan.role.${role}`)}
                ${Boolean(role === CLAN_MEMBER_ROLE.Leader) ? `(${$t('clan.member.update.transferRights')})` : ''}
              `,
              value: role,
            }))"
          />
        </UCard>

        <i18n-t
          v-if="canKick"
          scope="global"
          keypath="clan.member.kick.title"
          tag="p"
          class="text-center text-xs text-content-300"
        >
          <template #memberLink>
            <AppConfirmActionPopover @confirm="$emit('kick')">
              <span class="cursor-pointer text-error">
                {{ $t('clan.member.kick.memberLink') }}
              </span>
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
          @click="$emit('cancel')"
        />

        <UButton
          size="xl"
          :label="$t('action.save')"
          :disabled="member.role === memberRoleModel"
          data-aq-clan-member-action="save"
          @click="onSave"
        />
      </template>
    </UModal>

    <AppConfirmActionDialog
      v-if="canUpdate"
      :open="shownConfirmTransferDialog"
      :title="$t('clan.member.update.confirmationDialog.title')"
      :description="$t('clan.member.update.confirmationDialog.desc')"
      :name="member.user.name"
      :confirm-label="$t('action.confirm')"
      @cancel="toggleConfirmTransferDialog(false);"
      @confirm="() => {
        $emit('update', memberRoleModel);
        toggleConfirmTransferDialog(false);
      }"
      @update:open="toggleConfirmTransferDialog(false)"
    />
  </div>
</template>
