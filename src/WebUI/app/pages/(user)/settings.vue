<script setup lang="ts">
import { LazyAppConfirmActionDialog } from '#components'

import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { logout } from '~/services/auth-service'
import { getCharacters } from '~/services/character-service'
import { deleteUser as _deleteUser } from '~/services/user-service'

definePageMeta({
  roles: SomeRole,
})

const { user } = useUser()

const { state: characters, isLoading: loadingCharacters } = useAsyncState(() => getCharacters(), [])
const canDeleteUser = computed(() => !characters.value.length)

const { t, n } = useI18n()
const overlay = useOverlay()
const confirmDeleteDialog = overlay.create(LazyAppConfirmActionDialog, {
  props: {
    title: t('user.settings.delete.dialog.title'),
    description: t('user.settings.delete.dialog.desc'),
    confirm: t('user.settings.delete.dialog.enterToConfirm', {
      userName: user.value!.name,
      heirloomPoints: user.value!.heirloomPoints,
      gold: n(user.value!.gold),
    }),
  },
})

const [onDeleteUser] = useAsyncCallback(async () => {
  await _deleteUser()
  logout()
}, {
  successMessage: 'user.settings.delete.notify.success',
})

async function deleteUser() {
  if (!(await confirmDeleteDialog.open())) {
    return
  }

  onDeleteUser()
}
</script>

<template>
  <UContainer
    class="
      py-8
      md:py-16
    "
  >
    <div
      class="
        mx-auto
        md:max-w-2xl
      "
    >
      <UiHeading :title="$t('user.settings.title')" class="mb-14 text-center" />

      <UiCard
        :ui="{ body: 'relative min-h-24', header: 'text-error' }"
        icon="crpg:alert-circle"
        :label="$t('user.settings.dangerZone')"
      >
        <UiLoading v-if="loadingCharacters" active />

        <template v-else>
          <div
            v-if="!canDeleteUser"
            class="mb-5 text-warning"
            data-aq-cant-delete-user-message
          >
            {{ $t('user.settings.delete.validation.hasChar') }}
          </div>

          <i18n-t
            scope="global"
            keypath="user.settings.delete.title"
            data-aq-delete-user-group
            tag="div"
            :class="{ 'pointer-events-none opacity-30': !canDeleteUser }"
          >
            <template #link>
              <ULink
                class="
                  cursor-pointer text-error
                  hover:text-error/80
                "
                @click="deleteUser"
              >
                {{ $t('user.settings.delete.link') }}
              </ULink>
            </template>
          </i18n-t>
        </template>
      </UiCard>
    </div>
  </UContainer>
</template>
