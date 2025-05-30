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
  <UModal
    title="TODO:"
    :ui="{ footer: 'justify-center' }"
    :close="{
      size: 'sm',
      color: 'secondary',
      variant: 'solid',
    }"
    @update:open="$emit('cancel')"
  >
    <template #title>
      <UserMedia :user="member.user" size="xl" class="my-3" />
    </template>

    <template #body>
      <div v-if="canUpdate">
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

      <USeparator class="h-8" />

      <div
        v-if="canKick"
        class="py-3"
      >
        <i18n-t
          scope="global"
          keypath="clan.member.kick.title"
          tag="p"
          class="text-center text-xs text-content-300"
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
                      @click="() => {
                        hide();
                        $emit('kick');
                      }"
                    />
                  </div>
                </div>
              </template>
            </UiModal>
          </template>
        </i18n-t>
      </div>
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

    <UiModal
      v-if="canUpdate"
      :shown="shownConfirmTransferDialog"
      data-aq-clan-member-action="confirm-transfer-dialog"
      @hide="toggleConfirmTransferDialog(false)"
    >
      <template #popper="{ hide }">
        <AppConfirmActionForm
          :title="$t('clan.member.update.confirmationDialog.title')"
          :description="$t('clan.member.update.confirmationDialog.desc')"
          :confirm-label="$t('action.confirm')"
          :name="member.user.name"
          @cancel="hide"
          @confirm="() => {
            $emit('update', memberRoleModel);
            hide();
          }"
        />
      </template>
    </UiModal>
  </UModal>
</template>
