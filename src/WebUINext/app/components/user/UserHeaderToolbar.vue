<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import NumberFlow, { continuous } from '@number-flow/vue'

import type { User } from '~/models/user'

import { Role } from '~/models/role'
import { logout } from '~/services/auth-service'
import { mapUserToUserPublic } from '~/services/user-service'

const { user } = defineProps<{
  user: User
}>()

const userStore = useUserStore()

const { t, locale, availableLocales, setLocale } = useI18n()

const items = computed<DropdownMenuItem[][]>(() => [
  [
    {
      label: `${t('setting.language')} | ${locale.value.toUpperCase()}`,
      icon: `crpg:${locale.value}`,
      children: availableLocales.map(l => ({
        label: t(`locale.${l}`),
        type: 'checkbox' as const,
        icon: `crpg:${l}`,
        checked: l === locale.value,
        onUpdateChecked() {
          setLocale(l)
        },
      })),
    },
    {
      label: t('setting.notifications'),
      icon: 'crpg:carillon',
      to: { name: 'notifications' },
      slot: 'notifications' as const,
    },
    {
      label: t('setting.settings'),
      icon: 'crpg:settings',
      to: { name: 'settings' },
    },
  ],
  [
    ...([Role.Moderator, Role.Admin].includes(userStore.user!.role))
      ? [{
          label: t('nav.main.Moderator'),
          to: { name: 'moderator' },
        } as DropdownMenuItem]
      : [],
    ...([Role.Admin].includes(userStore.user!.role))
      ? [{
          label: t('nav.main.Admin'),
          to: { name: 'admin' },
        } as DropdownMenuItem]
      : [],
  ],
  [
    {
      label: t('setting.logout'),
      icon: 'crpg:logout',
      onSelect: logout,
    },
  ],
])
</script>

<template>
  <div class="flex items-center gap-3">
    <!-- TODO: FIXME: condition -->
    <UButton
      size="md"
      variant="subtle"
      icon="crpg:gift"
      :label="$t('welcome.shortTitle')"
    />

    <AppCoin>
      <NumberFlow
        :value="user.gold"
        :plugins="[continuous]"
        locales="en-US"
        will-change
      />
    </AppCoin>

    <USeparator orientation="vertical" class="h-6" />

    <AppLoom :point="user.heirloomPoints" />

    <USeparator orientation="vertical" class="h-6" />

    <UserMedia
      :user="mapUserToUserPublic(user)"
      hidden-platform
    />

    <USeparator orientation="vertical" class="h-6" />

    <UDropdownMenu
      :modal="false"
      size="xl"
      :items
    >
      <UChip
        :show="Boolean(user.unreadNotificationsCount)"
        inset
        size="xl"
        :ui="{ base: 'bg-[#53bc96]' }"
      >
        <UButton
          size="md"
          variant="soft"
          color="secondary"
          icon="crpg:dots"
        />
      </UChip>

      <template #notifications-leading>
        <UChip
          :show="Boolean(user.unreadNotificationsCount)"
          inset
          :ui="{ base: 'bg-[#53bc96]' }"
        >
          <UIcon name="crpg:carillon" class="size-[1.125rem]" />
        </UChip>
      </template>
    </UDropdownMenu>
  </div>
</template>
