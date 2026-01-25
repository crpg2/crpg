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
        class="squircle rounded-none"
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
            :class="[labelClasses(), { 'text-primary': isSelf }]"
            :title="user.name"
          >
            {{ user.name }}
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

<style>
.squircle {
  mask-image: url("data:image/svg+xml,%3csvg width='200' height='200' xmlns='http://www.w3.org/2000/svg'%3e%3cpath d='M100 0C20 0 0 20 0 100s20 100 100 100 100-20 100-100S180 0 100 0Z'/%3e%3c/svg%3e");
  mask-size: contain;
  mask-position: center;
  mask-repeat: no-repeat;
}
</style>
