<script setup lang="ts">
import type { ClanMemberRole } from '~/models/clan'
import type { UserPublic } from '~/models/user'

const {
  clanRole = null,
  hiddenClan = false,
  hiddenPlatform = false,
  hiddenTitle = false,
  isSelf = false,
  size = 'sm',
  user,
} = defineProps<{
  user: UserPublic
  clanRole?: ClanMemberRole | null
  isSelf?: boolean
  hiddenPlatform?: boolean
  hiddenTitle?: boolean
  hiddenClan?: boolean
  size?: 'sm' | 'xl'
}>()
</script>

<template>
  <div class="inline-flex items-center gap-1 align-middle">
    <img
      :src="user.avatar"
      class="rounded-full"
      :alt="user.name"
      :class="[size === 'xl' ? 'size-8' : 'size-6', { 'ring-2  ring-status-success': isSelf }]"
    >

    <UserClan
      v-if="!hiddenClan && user.clan"
      :clan="user.clan"
      :clan-role="clanRole"
    />

    <div
      v-if="!hiddenTitle"
      class="max-w-full truncate"
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
      :platform-user-id="user.platformUserId"
      :user-name="user.name"
      :size="size"
    />
  </div>
</template>
