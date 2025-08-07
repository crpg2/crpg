<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import NumberFlow from '@number-flow/vue'

import type { Role } from '~/models/role'
import type { User } from '~/models/user'

import { ROLE } from '~/models/role'
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
    ...(([ROLE.Moderator, ROLE.Admin] as Role[]).includes(userStore.user!.role))
      ? [{
          label: t('nav.main.Moderator'),
          to: { name: 'moderator' },
        } as DropdownMenuItem]
      : [],
    ...(userStore.user!.role === ROLE.Admin)
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
      variant="subtle"
      icon="crpg:gift"
      :label="$t('welcome.shortTitle')"
    />
    <!-- TODO: FIXME: condition -->

    <UTooltip :text="$t('user.field.gold')">
      <AppCoin>
        <NumberFlow
          :value="user.gold"
          locales="en-US"
        />
      </AppCoin>
    </UTooltip>

    <UTooltip :text="$t('user.field.heirloom')">
      <AppLoom :point="user.heirloomPoints" />
    </UTooltip>

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
      <template #default="{ open }">
        <UChip
          :show="Boolean(user.unreadNotificationsCount)"
          inset
          size="xl"
          :ui="{ base: 'bg-[var(--color-notification)]' }"
        >
          <UButton
            variant="outline"
            color="neutral"
            icon="crpg:dots"
            active-variant="subtle"
            :active="open"
          />
        </UChip>
      </template>

      <template #notifications-leading>
        <UChip
          :show="Boolean(user.unreadNotificationsCount)"
          inset
          :ui="{ base: 'bg-[var(--color-notification)]' }"
        >
          <UIcon name="crpg:carillon" class="size-[1.125rem]" />
        </UChip>
      </template>
    </UDropdownMenu>
  </div>
</template>
