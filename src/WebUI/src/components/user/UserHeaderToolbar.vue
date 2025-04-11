<script setup lang="ts">
import { useTransition } from '@vueuse/core'

import { logout } from '~/services/auth-service'
import { mapUserToUserPublic } from '~/services/users-service'
import { useUserStore } from '~/stores/user'

const userStore = useUserStore()

const animatedUserGold = useTransition(toRef(() => userStore.user!.gold))
</script>

<template>
  <div class="gap flex items-center gap-3">
    <!-- TODO: improve tooltip, share heirloom, bla bla bla -->
    <Coin
      v-tooltip.bottom="$t('user.field.gold')"
      :value="Number(animatedUserGold.toFixed(0))"
    />

    <Divider inline />

    <Heirloom
      v-tooltip.bottom="$t('user.field.heirloom')"
      :value="userStore.user!.heirloomPoints"
    />

    <Divider inline />

    <UserMedia
      :user="mapUserToUserPublic(userStore.user!, userStore.clan)"
      :clan="userStore.clan"
      :clan-role="userStore.clanMemberRole"
      hidden-platform
      size="xl"
    />

    <Divider inline />

    <VDropdown placement="bottom-end">
      <template #default="{ shown }">
        <OButton :variant="shown ? 'transparent-active' : 'transparent'" size="sm" rounded>
          <FontAwesomeLayers full-width class="fa-2x">
            <FontAwesomeIcon :icon="['crpg', 'dots']" />
            <FontAwesomeLayersText
              v-if="userStore.hasUnreadNotifications"
              counter
              value="●"
              position="top-right"
              :style="{ '--fa-counter-background-color': 'rgba(83, 188, 150, 1)' }"
            />
          </FontAwesomeLayers>
        </OButton>
      </template>

      <template #popper="{ hide }">
        <SwitchLanguageDropdown
          v-slot="{ shown, locale }"
          placement="left-start"
        >
          <DropdownItem :active="shown">
            <SvgSpriteImg
              :name="`locale-${locale}`"
              viewBox="0 0 18 18"
              class="w-4.5"
            />
            {{ $t('setting.language') }} | {{ locale.toUpperCase() }}
          </DropdownItem>
        </SwitchLanguageDropdown>

        <DropdownItem tag="RouterLink" :to="{ name: 'Notifications' }" @click="hide">
          <FontAwesomeLayers full-width class="fa-sm">
            <FontAwesomeIcon :icon="['crpg', 'carillon']" />
            <FontAwesomeLayersText
              v-if="userStore.hasUnreadNotifications"
              counter
              value="●"
              position="top-right"
              :style="{ '--fa-counter-background-color': 'rgba(83, 188, 150, 1)' }"
            />
          </FontAwesomeLayers>
          <div>{{ $t('setting.notifications') }}</div>
        </DropdownItem>

        <DropdownItem
          tag="RouterLink"
          :to="{ name: 'Settings' }"
          icon="settings"
          :label="$t('setting.settings')"
          @click="hide"
        />

        <DropdownItem
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
