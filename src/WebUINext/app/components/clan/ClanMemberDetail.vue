<script setup lang="ts">
import type { ClanMember } from '~/models/clan'

import { ClanMemberRole } from '~/models/clan'

const props = defineProps<{
  member: ClanMember
  canUpdate: boolean
  canKick: boolean
}>()

const emit = defineEmits<{
  kick: []
  update: [role: ClanMemberRole]
  cancel: []
}>()

const memberRoleModel = ref<ClanMemberRole>(props.member.role)

const [shownConfirmTransferDialog, toggleConfirmTransferDialog] = useToggle()

const onSave = () => {
  if (memberRoleModel.value === ClanMemberRole.Leader) {
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
    <div
      class="flex items-center justify-center gap-2.5 border-b border-border-200 px-12 pb-8 pt-11 text-content-100"
    >
      <UserMedia
        :user="member.user"
        size="xl"
      />
    </div>

    <div
      v-if="canUpdate"
      class="mt-8 border-b border-border-200 px-8 pb-8"
    >
      <UiFormGroup
        icon="member"
        :label="$t('clan.roleTitle')"
        :collapsable="false"
        :bordered="false"
      >
        <ORadio
          v-for="role in Object.keys(ClanMemberRole)"
          :key="role"
          v-model="memberRoleModel"
          :native-value="role"
        >
          {{ $t(`clan.role.${role}`) }}
          <template v-if="role === ClanMemberRole.Leader">
            ({{ $t('clan.member.update.transferRights') }})
          </template>
        </ORadio>
      </UiFormGroup>
    </div>

    <div
      v-if="canKick"
      class="border-b border-border-200 px-12 py-8"
    >
      <i18n-t
        scope="global"
        keypath="clan.member.kick.title"
        tag="p"
        class="text-center text-2xs text-content-300"
      >
        <template #memberLink>
          <UiModal>
            <span class="cursor-pointer text-status-danger">
              {{ $t('clan.member.kick.memberLink') }}
            </span>

            <template #popper="{ hide }">
              <div class="px-12 py-11 text-center">
                <i18n-t
                  scope="global"
                  keypath="clan.member.kick.dialog.title"
                  tag="div"
                  class="mb-8 flex items-center justify-center gap-2 text-xl text-content-100"
                >
                  <template #memberName>
                    <UserMedia
                      :user="member.user"
                      hidden-platform
                      size="xl"
                    />
                  </template>
                </i18n-t>

                <p>{{ $t('clan.member.kick.dialog.desc') }}</p>

                <div class="mt-8 flex items-center justify-center gap-4">
                  <OButton
                    variant="primary"
                    outlined
                    size="xl"
                    data-aq-clan-member-action="kick-cancel"
                    :label="$t('action.cancel')"
                    @click="hide"
                  />
                  <OButton
                    variant="primary"
                    size="xl"
                    :label="$t('action.kick')"
                    data-aq-clan-member-action="kick"
                    @click="
                      () => {
                        hide();
                        emit('kick');
                      }
                    "
                  />
                </div>
              </div>
            </template>
          </UiModal>
        </template>
      </i18n-t>
    </div>

    <div class="mt-8 flex items-center justify-center gap-4 px-12 pb-3">
      <OButton
        variant="primary"
        outlined
        size="xl"
        :label="$t('action.cancel')"
        data-aq-clan-member-action="close-detail"
        @click="emit('cancel')"
      />

      <OButton
        variant="primary"
        size="xl"
        :label="$t('action.save')"
        :disabled="member.role === memberRoleModel"
        data-aq-clan-member-action="save"
        @click="onSave"
      />
    </div>

    <Modal
      v-if="canUpdate"
      :shown="shownConfirmTransferDialog"
      data-aq-clan-member-action="confirm-transfer-dialog"
      @hide="toggleConfirmTransferDialog(false)"
    >
      <template #popper="{ hide }">
        <AppConfirmActionForm
          :title="$t('clan.member.update.confirmationDialog.title')"
          :description="$t('clan.member.update.confirmationDialog.desc')"
          :name="member.user.name"
          :confirm-label="$t('action.confirm')"
          @cancel="hide"
          @confirm="
            () => {
              hide();
              emit('update', memberRoleModel);
            }
          "
        />
      </template>
    </Modal>
  </div>
</template>
