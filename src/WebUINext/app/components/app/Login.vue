<script setup lang="ts">
import type { ButtonProps, DropdownMenuItem } from '@nuxt/ui'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { PLATFORM } from '~/models/platform'
import { login } from '~/services/auth-service'
import { platformToIcon } from '~/services/platform-service'
import { useUserStore } from '~/stores/user'

const { size = 'xl' } = defineProps<{ size?: ButtonProps['size'] }>()
const userStore = useUserStore()
const { user } = toRefs(userStore)
const { platform, changePlatform } = usePlatform()
const { t } = useI18n()

const {
  execute: loginUser,
  isLoading: logging,
} = useAsyncCallback(() => login(platform.value))

const items = computed(() =>
  Object.values(PLATFORM).map(p => ({
    label: t(`platform.${p}`),
    icon: `crpg:${platformToIcon[p]}`,
    type: 'checkbox' as const,
    checked: p === platform.value,
    onUpdateChecked() {
      changePlatform(p)
    },
  })) satisfies DropdownMenuItem[],
)
</script>

<template>
  <UFieldGroup
    v-if="!user"
    :size
  >
    <UButton
      :icon="`crpg:${platformToIcon[platform]}`"
      :loading="logging"
      @click="loginUser"
    >
      <div class="flex flex-col text-left leading-tight">
        <span class="text-3xs">{{ $t('login.label') }}</span>
        <span>{{ $t(`platform.${platform}`) }}</span>
      </div>
    </UButton>

    <UDropdownMenu
      size="md"
      :items
      :modal="false"
    >
      <template #default="{ open }">
        <UButton :icon="open ? 'crpg:chevron-up' : 'crpg:chevron-down'" />
      </template>
    </UDropdownMenu>
  </UFieldGroup>

  <UButton
    v-else
    :to="{ name: 'characters' }"
    data-aq-character-link
    size="xl"
    icon="crpg:member"
    :label="$t('action.enter')"
  />
</template>
