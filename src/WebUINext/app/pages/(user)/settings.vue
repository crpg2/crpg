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
  <UContainer>
    <div class="mx-auto max-w-2xl py-12">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('user.settings.title') }}
      </h1>

      <UCard :ui="{ body: 'relative min-h-24' }">
        <template #header>
          <UiDataCell class="w-full text-sm text-error">
            <template #leftContent>
              <UIcon name="crpg:alert-circle" class="size-6" />
            </template>
            {{ $t('user.settings.dangerZone') }}
          </UiDataCell>
        </template>

        <UiLoading v-if="userStore.fetchingCharacters" active />

        <template v-else>
          <div
            v-if="!canDeleteUser"
            class="mb-5 text-status-warning"
            data-aq-cant-delete-user-message
          >
            {{ $t('user.settings.delete.validation.hasChar') }}
          </div>

          <i18n-t
            scope="global"
            keypath="user.settings.delete.title"
            tag="div"
            class="prose leading-relaxed prose-invert"
            :class="{ 'pointer-events-none opacity-30': !canDeleteUser }"
            data-aq-delete-user-group
          >
            <template #link>
              <span
                class="cursor-pointer border-b border-dashed border-status-danger text-status-danger hover:border-0"
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
          class="prose prose-invert prose-h4:text-status-danger prose-h5:text-status-danger"
          v-html="$t('user.settings.delete.dialog.title')"
        />
      </template>
      <template #description>
        <p class="leading-relaxed text-status-warning">
          {{ $t('user.settings.delete.dialog.desc') }}
        </p>
      </template>
    </AppConfirmActionDialog>
  </UContainer>
</template>
