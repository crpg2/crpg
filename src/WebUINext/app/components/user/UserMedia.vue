<script setup lang="ts">
import type { AvatarProps } from '@nuxt/ui'

import { tv } from 'tailwind-variants'

import type { UserPublic } from '~/models/user'

type Size = 'sm' | 'md' | 'lg' | 'xl'

const {
  user,
  hiddenClan = false,
  hiddenPlatform = false,
  hiddenTitle = false,
  isSelf = false,
  size = 'md',
} = defineProps<{
  user: UserPublic
  isSelf?: boolean
  hiddenPlatform?: boolean
  hiddenTitle?: boolean
  hiddenClan?: boolean
  size?: Size
}>()

const variants = tv({
  slots: {
    name: '',
  },
  variants: {
    size: {
      sm: {
        name: '',
      },
      md: {
        name: '',
      },
      lg: {
        name: '',
      },
      xl: {
        name: 'text-lg',
      },
    },
  },
})

const avatarSize = computed(() => ({
  sm: 'xs',
  md: 'lg',
  lg: 'xl',
  xl: '3xl',
} satisfies Record<Size, AvatarProps['size']>)[size])

const classes = computed(() => variants({ size }))
</script>

<template>
  <UiDataCell inline>
    <template #leftContent>
      <UAvatar
        :src="user.avatar || ''"
        :size="avatarSize"
        :alt="user.name"
        :class="[{ 'ring-2 ring-success': isSelf }]"
      />
    </template>

    <div>
      <UiDataMedia layout="reverse">
        <template #icon>
          <UserPlatform
            v-if="hiddenPlatform"
            :platform="user.platform"
            size="sm"
            :platform-user-id="user.platformUserId"
            :user-name="user.name"
          />
        </template>

        <div
          v-if="!hiddenTitle"
          class="max-w-52 truncate"
          :class="classes.name()"
          :title="user.name"
        >
          {{ user.name }}
          <template v-if="isSelf">
            ({{ $t('you') }})
          </template>
        </div>
      </UiDataMedia>

      <div>
        <UserClan
          v-if="!hiddenClan && user.clanMembership"
          :clan="user.clanMembership.clan"
          size="sm"
          :clan-role="user.clanMembership.role"
        />
      </div>
    </div>
  </UiDataCell>
</template>
