<script setup lang="ts">
import type { ButtonProps, DropdownMenuItem } from '@nuxt/ui'

import { useStorage } from '@vueuse/core'

import type { Platform } from '~/models/platform'

import { useUser } from '~/composables/user/use-user'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { AVAILABLE_PLATFORM, PLATFORM } from '~/models/platform'
import { login } from '~/services/auth-service'
import { platformToIcon } from '~/services/platform-service'

const { size = 'xl' } = defineProps<{ size?: ButtonProps['size'] }>()
const { user } = useUser()

const platform = useStorage<Platform>('user-platform', PLATFORM.Steam)

const { t } = useI18n()

const [loginUser, logging] = useAsyncCallback(() => login(platform.value))

const items = AVAILABLE_PLATFORM.map(p => ({
  label: t(`platform.${p}`),
  icon: `crpg:${platformToIcon[p]}`,
  type: 'checkbox' as const,
  checked: p === platform.value,
  onUpdateChecked() {
    platform.value = p
  },
})) satisfies DropdownMenuItem[]
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
        <span class="text-xs">{{ $t('login.label') }}</span>
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
