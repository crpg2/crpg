<script setup lang="ts">
import type { UserRestrictionCreation } from '~/models/user'

import { useModerationUser } from '~/composables/moderator/use-moderation-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getUserRestrictions, restrictUser } from '~/services/restriction-service'

const { t } = useI18n()
const toast = useToast()
const { moderationUser } = useModerationUser()

const {
  state: restrictions,
  execute: loadRestrictions,
  isLoading: loadingRestrictions,
} = useAsyncState(() => getUserRestrictions(moderationUser.value.id), [], { resetOnExecute: false })

const {
  execute: onRestrictUser,
  isLoading: restrictingUser,
} = useAsyncCallback(async (restriction: Omit<UserRestrictionCreation, 'restrictedUserId'>) => {
  await restrictUser({
    ...restriction,
    restrictedUserId: moderationUser.value.id,
  })
  await loadRestrictions()

  toast.add({
    title: t('restriction.create.notify.success'),
    color: 'success',
    close: false,
  })
})
</script>

<template>
  <div>
    <div class="mb-8 flex items-center gap-4">
      <h2 class="text-lg">
        {{ $t('restriction.user.history') }}
      </h2>

      <UModal
        :title="$t('restriction.create.form.title')"
        :close="{
          size: 'sm',
          color: 'secondary',
          variant: 'solid',
        }"
      >
        <UButton
          size="sm"
          variant="subtle"
          icon="crpg:plus"
          :label="$t('restriction.create.form.title')"
        />

        <template #body="{ close }">
          <ModeratorCreateRestrictionForm
            :loading="restrictingUser"
            @submit="(data) => {
              onRestrictUser(data);
              close();
            }"
          />
        </template>
      </UModal>
    </div>

    <ModeratorRestrictionsTable
      :restrictions="restrictions"
      :loading="loadingRestrictions"
      hidden-restricted-user
    />
  </div>
</template>
