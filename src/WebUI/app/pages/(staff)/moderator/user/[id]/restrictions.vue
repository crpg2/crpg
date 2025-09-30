<script setup lang="ts">
import type { UserRestrictionCreation } from '~/models/user'

import { useModerationUser } from '~/composables/moderator/use-moderation-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getUserRestrictions, restrictUser } from '~/services/restriction-service'

const { t } = useI18n()
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
}, {
  pageLoading: true,
  successMessage: t('restriction.create.notify.success'),
})
</script>

<template>
  <div>
    <div class="mb-8 flex items-center gap-4">
      <UiTextView variant="h2" tag="h2">
        {{ $t('restriction.user.history') }}
      </UiTextView>

      <USeparator orientation="vertical" class="h-6" />

      <UModal :title="$t('restriction.create.form.title')">
        <UButton
          variant="subtle"
          icon="crpg:plus"
          :label="$t('restriction.create.form.title')"
        />

        <template #body="{ close }">
          <LazyModeratorCreateRestrictionForm
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
