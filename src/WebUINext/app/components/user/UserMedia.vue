<script setup lang="ts">
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

// TODO: use avatar props when resolve this issue https://github.com/nuxt/ui/issues/3973
type AvatarSize = '3xs' | '2xs' | 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl' | '3xl'

const avatarSize = computed(() => ({ sm: 'xs', md: 'md', lg: 'xl', xl: '3xl' } satisfies Record<Size, AvatarSize>)[size])

const classes = computed(() => variants({ size }))
</script>

<template>
  <div class="inline-flex items-center gap-1.5 align-middle">
    <UAvatar
      :src="user.avatar || ''"
      :size="avatarSize"
      :alt="user.name"
      :class="[{ 'ring-2 ring-status-success': isSelf }]"
    />

    <UserClan
      v-if="!hiddenClan && user.clanMembership"
      :clan="user.clanMembership.clan"
      :size
      :clan-role="user.clanMembership.role"
    />

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

    <UserPlatform
      v-if="!hiddenPlatform"
      :platform="user.platform"
      :size
      :platform-user-id="user.platformUserId"
      :user-name="user.name"
    />
  </div>
</template>
