<script setup lang="ts">
import { NuxtLink, UiDropdownItem } from '#components'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { Platform } from '~/models/platform'
import { login } from '~/services/auth-service'
import { platformToIcon } from '~/services/platform-service'
import { useUserStore } from '~/stores/user'

const { size = 'xl' } = defineProps<{ size?: 'sm' | 'xl' }>()
const userStore = useUserStore()
const { user } = toRefs(userStore)
const { platform, changePlatform } = usePlatform()

const {
  execute: loginUser,
  loading: logging,
} = useAsyncCallback(() => login(platform.value))
</script>

<template>
  <OField v-if="user === null">
    <OButton
      variant="primary"
      :size
      :icon-left="platformToIcon[platform]"
      :loading="logging"
      data-aq-login-btn
      @click="loginUser"
    >
      <div class="flex flex-col text-left leading-tight">
        <span class="text-[10px]">{{ $t('login.label') }}</span>
        <span>{{ $t(`platform.${platform}`) }}</span>
      </div>
    </OButton>

    <VDropdown
      :triggers="['click']"
      placement="bottom-end"
    >
      <template #default="{ shown }">
        <OButton
          variant="primary"
          :icon-right="shown ? 'chevron-up' : 'chevron-down'"
          :size
        />
      </template>

      <template #popper="{ hide }">
        <UiDropdownItem
          v-for="p in Object.values(Platform)"
          :key="p"
          :checked="p === platform"
          :label="$t(`platform.${p}`)"
          :icon="platformToIcon[p]"
          data-aq-platform-item
          @click="
            () => {
              changePlatform(p);
              hide();
            }
          "
        />
      </template>
    </VDropdown>
  </OField>

  <NuxtLink
    v-else
    :to="{ name: 'characters' }"
    data-aq-character-link
  >
    <OButton
      variant="primary"
      size="xl"
      icon-left="member"
      :label="$t('action.enter')"
    />
  </NuxtLink>
</template>
