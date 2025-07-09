<script setup lang="ts">
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { SomeRole } from '~/models/role'
import { logout } from '~/services/auth-service'
import { deleteUser } from '~/services/user-service'
import { useUserStore } from '~/stores/user'

definePageMeta({
  roles: SomeRole,
})

const { t } = useI18n()
const toast = useToast()

const userStore = useUserStore()

const canDeleteUser = computed(() => !userStore.characters.length)

const { execute: onDeleteUser } = useAsyncCallback(async () => {
  await deleteUser()
  toast.add({
    title: t('user.settings.delete.notify.success'),
    color: 'success',
    close: false,
  })
  logout()
})

userStore.fetchCharacters()

const [shownConfirmDialog, toggleConfirmDialog] = useToggle()
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

      <UCard :ui="{ body: 'relative min-h-24' }" variant="outline">
        <template #header>
          <UiDataCell class="w-full text-sm text-error">
            <template #leftContent>
              <UIcon name="crpg:alert-circle" class="size-8" />
            </template>
            <h4 class="text-lg font-bold">
              {{ $t('user.settings.dangerZone') }}
            </h4>
          </UiDataCell>
        </template>

        <UiLoading v-if="userStore.fetchingCharacters" active />

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
            tag="div"
            class="prose leading-relaxed"
            :class="{ 'pointer-events-none opacity-30': !canDeleteUser }"
            data-aq-delete-user-group
          >
            <template #link>
              <span
                class="
                  cursor-pointer border-b border-dashed text-error
                  hover:border-0
                "
                @click="() => toggleConfirmDialog()"
              >
                {{ $t('user.settings.delete.link') }}
              </span>
            </template>
          </i18n-t>
        </template>
      </UCard>
    </div>

    <AppConfirmActionDialog
      v-if="shownConfirmDialog"
      open
      :name="$t('user.settings.delete.dialog.enterToConfirm', { user: userStore.user!.name })"
      :confirm-label="$t('action.delete')"
      @cancel="toggleConfirmDialog(false);"
      @confirm="() => {
        onDeleteUser();
        toggleConfirmDialog(false);
      }"
      @update:open="toggleConfirmDialog(false)"
    >
      <template #title>
        <div
          class="
            prose
            prose-h4:text-status-danger
            prose-h5:text-status-danger
          "
          v-html="$t('user.settings.delete.dialog.title')"
        />
      </template>
      <template #description>
        <p class="text-status-warning leading-relaxed">
          {{ $t('user.settings.delete.dialog.desc') }}
        </p>
      </template>
    </AppConfirmActionDialog>
  </UContainer>
</template>
