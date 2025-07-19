<script setup lang="ts">
import { tv } from 'tailwind-variants'

import type { ClanMemberRole, ClanPublic } from '~/models/clan'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

type Size = 'sm' | 'md' | 'lg' | 'xl'

const { clan, clanRole, size = 'md' } = defineProps<{
  clan: ClanPublic
  clanRole?: ClanMemberRole | null
  size?: Size
}>()

const variants = tv({
  slots: {
    name: '',
    roleIcon: '',
    tagIcon: '',
  },
  variants: {
    size: {
      sm: {
        name: '',
        roleIcon: '',
        tagIcon: '',
      },
      md: {
        name: '',
        roleIcon: 'size-5',
        tagIcon: 'size-5',
      },
      lg: {
        name: 'text-sm',
        roleIcon: 'size-6',
        tagIcon: 'size-6',
      },
      xl: {
        name: 'text-lg',
        roleIcon: 'size-8',
        tagIcon: 'size-8',
      },
    },
  },
})
const classes = computed(() => variants({ size }))
</script>

<template>
  <NuxtLink
    class="
      inline-flex items-center gap-1.5 align-middle
      hover:text-highlighted
    "
    :to="{ name: 'clans-id', params: { id: clan.id } }"
  >
    <UTooltip
      v-if="clanRole && ([CLAN_MEMBER_ROLE.Leader, CLAN_MEMBER_ROLE.Officer] as ClanMemberRole[]).includes(clanRole)"
      :text="$t(`clan.role.${clanRole}`)"
    >
      <ClanRole
        :role="clanRole"
        :size
        hidden-label
      />
    </UTooltip>

    <ClanTagIcon
      :color="clan.primaryColor"
      :class="classes.tagIcon()"
    />

    <span :class="classes.name()">
      [{{ clan.tag }}]
    </span>
  </NuxtLink>
</template>
