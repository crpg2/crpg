<script setup lang="ts">
import type { AvatarProps } from '@nuxt/ui'

import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'
import type { UserPublic } from '~/models/user'

const {
  user,
  hiddenClan = false,
  hiddenPlatform = false,
  hiddenTitle = false,
  isSelf = false,
  inline = false,
  size = 'md',
} = defineProps<{
  user: UserPublic
  isSelf?: boolean
  inline?: boolean
  hiddenPlatform?: boolean
  hiddenTitle?: boolean
  hiddenClan?: boolean
  size?: DataMediaSize
}>()

const avatarSize = computed(() => ({
  xs: 'xs',
  sm: 'xs',
  md: 'lg',
  lg: 'xl',
  xl: '3xl',
} satisfies Record<DataMediaSize, AvatarProps['size']>)[size])

const clanSize = computed(() => ({
  xs: 'xs',
  sm: 'xs',
  md: 'sm',
  lg: 'md',
  xl: 'lg',
} satisfies Record<DataMediaSize, DataMediaSize>)[size])
</script>

<template>
  <UiDataCell :inline>
    <template #leftContent>
      <UAvatar
        :src="user.avatar || ''"
        :size="avatarSize"
        :alt="user.name"
        :class="[{ 'ring-2 ring-success': isSelf }]"
      />
    </template>

    <div class="flex flex-col items-start gap-0.5">
      <UiDataMedia layout="reverse" :size>
        <template
          v-if="!hiddenPlatform"
          #icon="{ classes: platformIconClasses }"
        >
          <UserPlatform
            :platform="user.platform"
            :class="platformIconClasses()"
            :platform-user-id="user.platformUserId"
            :user-name="user.name"
          />
        </template>

        <template
          v-if="!hiddenTitle"
          #default="{ classes: labelClasses }"
        >
          <div
            class="max-w-32 truncate leading-none"
            :class="labelClasses()"
            :title="user.name"
          >
            {{ user.name }}
            <template v-if="isSelf">
              ({{ $t('you') }})
            </template>
          </div>
        </template>
      </UiDataMedia>

      <UserClan
        v-if="!hiddenClan && user.clanMembership"
        :clan="user.clanMembership.clan"
        :size="clanSize"
        :clan-role="user.clanMembership.role"
      />
    </div>
  </UiDataCell>
</template>
