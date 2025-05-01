<script setup lang="ts">
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { logout } from '~/services/auth-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import { deleteUser } from '~/services/users-service'
import { useUserStore } from '~/stores/user'

definePageMeta({
  roles: SomeRole,
})

const userStore = useUserStore()

await userStore.fetchCharacters()

const canDeleteUser = computed(() => !userStore.characters.length)

const { execute: onDeleteUser } = useAsyncCallback(async () => {
  await deleteUser()
  notify(t('user.settings.delete.notify.success'))
  logout()
})
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-2xl py-12">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('user.settings.title') }}
      </h1>

      <FormGroup
        icon="alert-circle"
        :label="$t('user.settings.dangerZone')"
        collapsed
        variant="danger"
      >
        <div
          v-if="!canDeleteUser"
          class="text-status-warning"
          data-aq-cant-delete-user-message
        >
          {{ $t('user.settings.delete.validation.hasChar') }}
        </div>

        <i18n-t
          scope="global"
          keypath="user.settings.delete.title"
          tag="div"
          class="prose prose-invert leading-relaxed"
          :class="{ 'pointer-events-none opacity-30': !canDeleteUser }"
          data-aq-delete-user-group
        >
          <template #link>
            <Modal :disabled="!canDeleteUser">
              <span
                class="cursor-pointer border-b border-dashed border-status-danger text-status-danger hover:border-0"
              >
                {{ $t('user.settings.delete.link') }}
              </span>

              <template #popper="{ hide }">
                <ConfirmActionForm
                  :name="
                    $t('user.settings.delete.dialog.enterToConfirm', { user: userStore.user!.name })
                  "
                  :confirm-label="$t('action.delete')"
                  no-select
                  @cancel="hide"
                  @confirm="
                    () => {
                      onDeleteUser();
                      hide();
                    }
                  "
                >
                  <template #title>
                    <div
                      class="prose prose-invert prose-h4:text-status-danger prose-h5:text-status-danger"
                      v-html="$t('user.settings.delete.dialog.title')"
                    />
                  </template>
                  <template #description>
                    <p class="leading-relaxed text-status-warning">
                      {{ $t('user.settings.delete.dialog.desc') }}
                    </p>
                  </template>
                </ConfirmActionForm>
              </template>
            </Modal>
          </template>
        </i18n-t>
      </FormGroup>
    </div>
  </div>
</template>
