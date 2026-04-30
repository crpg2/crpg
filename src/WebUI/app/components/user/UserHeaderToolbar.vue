<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import type { Role } from '~/models/role'
import type { User } from '~/models/user'

import { ROLE } from '~/models/role'
import { logout } from '~/services/auth-service'
import { mapUserToUserPublic } from '~/services/user-service'

const { user } = defineProps<{
  user: User
}>()

const emit = defineEmits<{
  showWelcome: []
}>()

const { t, locale, availableLocales, setLocale } = useI18n()

const items = computed<DropdownMenuItem[][]>(() => [
  [
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
    ...(user.isRecent
      ? [
          {
            label: t('welcome.shortTitle'),
            icon: 'crpg:gift',
            color: 'primary',
            onSelect: () => {
              emit('showWelcome')
            },
          } as DropdownMenuItem,
        ]
      : []),
  ],
  [
    ...(([ROLE.Moderator, ROLE.Admin] as Role[]).includes(user.role))
      ? [{
          label: t('nav.main.Moderator'),
          to: { name: 'moderator' },
        } as DropdownMenuItem]
      : [],
    ...(user.role === ROLE.Admin)
      ? [{
          label: t('nav.main.Admin'),
          to: { name: 'admin' },
        } as DropdownMenuItem]
      : [],
  ],
  [
    {
      label: `${t('setting.language')} | ${locale.value.toUpperCase()}`,
      icon: `crpg:locale-${locale.value}`,
      children: availableLocales.map(l => ({
        label: t(`locale.${l}`),
        type: 'checkbox' as const,
        icon: `crpg:locale-${l}`,
        checked: l === locale.value,
        onUpdateChecked() {
          setLocale(l)
        },
      })),
    },
    {
      label: t('setting.logout'),
      icon: 'crpg:logout',
      onSelect: logout,
    },
  ],
])
</script>

<template>
  <div class="flex items-center gap-4">
    <UChip
      :show="Boolean(user.unreadNotificationsCount)"
      inset
      :text="user.unreadNotificationsCount"
      size="2xl"
      :ui="{ base: 'bg-success text-white h-3.5 min-w-3.5 text-[8px] ring-0' }"
    >
      <UFieldGroup>
        <UTooltip
          :ui="{
            content: 'flex-col gap-4 justify-start items-start',
          }"
        >
          <UButton
            as="div"
            variant="soft"
            color="neutral"
          >
            <AppCoin :value="user.gold" compact>
              <template v-if="user.reservedGold" #trailing>
                <span class="text-xs text-dimmed">({{ $n(user.reservedGold, 'compact') }})</span>
              </template>
            </AppCoin>
          </UButton>

          <template #content>
            <UiDataCell>
              <template #leftContent>
                <UiTextView variant="p">
                  {{ t('user.field.gold') }}
                </UiTextView>
              </template>
              <AppCoin :value="user.gold" />
            </UiDataCell>

            <UiDataCell v-if="user.reservedGold">
              <template #leftContent>
                <UiTextView variant="p">
                  {{ t('user.field.reservedGold') }}
                </UiTextView>
              </template>
              <AppCoin :value="user.reservedGold" />
            </UiDataCell>
          </template>
        </UTooltip>

        <UTooltip
          :ui="{
            content: 'flex-col gap-4 justify-start items-start',
          }"
        >
          <UButton
            as="div"
            variant="soft"
            color="neutral"
          >
            <AppLoom :point="user.heirloomPoints">
              <template v-if="user.reservedHeirloomPoints" #trailing>
                <span class="text-xs text-dimmed">({{ $n(user.reservedHeirloomPoints, 'compact') }})</span>
              </template>
            </AppLoom>
          </UButton>

          <template #content>
            <UiDataCell>
              <template #leftContent>
                <UiTextView variant="p">
                  {{ t('user.field.heirloomPoints') }}
                </UiTextView>
              </template>
              <AppLoom :point="user.heirloomPoints" />
            </UiDataCell>

            <UiDataCell v-if="user.reservedHeirloomPoints">
              <template #leftContent>
                <UiTextView variant="p">
                  {{ t('user.field.reservedHeirloomPoints') }}
                </UiTextView>
              </template>
              <AppLoom :point="user.reservedHeirloomPoints" />
            </UiDataCell>
          </template>
        </UTooltip>

        <UButton
          as="div"
          variant="soft"
          color="neutral"
        >
          <UserMedia
            :user="mapUserToUserPublic(user)"
            hidden-platform
          />
        </UButton>

        <UDropdownMenu
          :modal="false"
          size="xl"
          :items
        >
          <template #default="{ open }">
            <UButton
              variant="soft"
              color="neutral"
              icon="i-lucide-ellipsis-vertical"
              active-variant="subtle"
              :active="open"
              size="xl"
            />
          </template>

          <template #notifications-leading>
            <UChip
              :show="Boolean(user.unreadNotificationsCount)"
              inset
              size="lg"
              :ui="{ base: 'bg-success text-white h-2.5 min-w-2.5 text-[7px]' }"
              :text="user.unreadNotificationsCount"
            >
              <UIcon name="crpg:carillon" class="size-4.5" />
            </UChip>
          </template>
        </UDropdownMenu>
      </UFieldGroup>
    </UChip>
  </div>
</template>
