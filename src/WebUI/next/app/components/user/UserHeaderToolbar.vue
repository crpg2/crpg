<script setup lang="ts">
import { useTransition } from '@vueuse/core'

import type { User } from '~/models/user'

import { logout } from '~/services/auth-service'
import { mapUserToUserPublic } from '~/services/user-service'

const { user } = defineProps<{
  user: User
}>()

const animatedUserGold = useTransition(() => user.gold)
</script>

<template>
  <div class="flex items-center gap-3">
    <AppCoin
      v-tooltip.bottom="$t('user.field.gold')"
      :value="Number(animatedUserGold.toFixed(0))"
    />

    <UiDivider inline />

    <AppLoom
      v-tooltip.bottom="$t('user.field.heirloom')"
      :point="user.heirloomPoints"
    />

    <UiDivider inline />

    <UserMedia
      :user="mapUserToUserPublic(user)"
      hidden-platform
      size="xl"
    />

    <UiDivider inline />

    <VDropdown placement="bottom-end">
      <template #default="{ shown }">
        <OButton :variant="shown ? 'transparent-active' : 'transparent'" size="sm" rounded>
          <FontAwesomeLayers full-width class="fa-2x">
            <FontAwesomeIcon :icon="['crpg', 'dots']" />
            <FontAwesomeLayersText
              v-if="user.unreadNotificationsCount"
              counter
              value="●"
              position="top-right"
              :style="{ '--fa-counter-background-color': 'rgba(83, 188, 150, 1)' }"
            />
          </FontAwesomeLayers>
        </OButton>
      </template>

      <template #popper="{ hide }">
        <AppSwitchLanguageDropdown
          v-slot="{ shown, locale }"
          placement="left-start"
        >
          <UiDropdownItem :active="shown">
            <SpriteSymbol
              :key="locale"
              :name="`locale/${locale}`"
              viewBox="0 0 18 18"
              inline
              class="w-4"
            />
            {{ $t('setting.language') }} | {{ locale.toUpperCase() }}
          </UiDropdownItem>
        </AppSwitchLanguageDropdown>

        <UiDropdownItem
          tag="NuxtLink"
          :to="{ name: 'Notifications' }"
          @click="hide"
        >
          <FontAwesomeLayers full-width class="fa-sm">
            <FontAwesomeIcon :icon="['crpg', 'carillon']" />
            <FontAwesomeLayersText
              v-if="user.unreadNotificationsCount"
              counter
              value="●"
              position="top-right"
              :style="{ '--fa-counter-background-color': 'rgba(83, 188, 150, 1)' }"
            />
          </FontAwesomeLayers>
          <div>{{ $t('setting.notifications') }}</div>
        </UiDropdownItem>

        <UiDropdownItem
          tag="NuxtLink"
          :to="{ name: 'Settings' }"
          icon="settings"
          :label="$t('setting.settings')"
          @click="hide"
        />

        <UiDropdownItem
          icon="logout"
          :label="$t('setting.logout')"
          @click="
            () => {
              hide();
              logout();
            }
          "
        />
      </template>
    </VDropdown>
  </div>
</template>
