<script setup lang="ts">
import { tv } from 'tailwind-variants'

import type { ClanMemberRole } from '~/models/clan'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

type Size = 'sm' | 'md' | 'lg' | 'xl'

const { hiddenLabel = false, size = 'md' } = defineProps<{
  hiddenLabel?: boolean
  role: ClanMemberRole
  size?: Size

}>()

const variants = tv({
  slots: {
    icon: '',
    label: 'font-bold',
  },
  variants: {
    size: {
      sm: {
        label: '',
        icon: '',
      },
      md: {
        label: '',
        icon: 'size-5',
      },
      lg: {
        label: 'text-sm',
        icon: 'size-6',
      },
      xl: {
        label: 'text-lg',
        icon: 'size-8',
      },
    },
  },
})

const classes = computed(() => variants({ size }))
</script>

<template>
  <div
    class="inline-flex items-center gap-0.5 align-middle"
    :class="
      role === CLAN_MEMBER_ROLE.Leader
        ? 'text-[#C99E34]'
        : role === CLAN_MEMBER_ROLE.Officer
          ? 'text-highlighted'
          : 'text-dimmed'
    "
  >
    <UIcon
      v-if="([CLAN_MEMBER_ROLE.Leader, CLAN_MEMBER_ROLE.Officer]as ClanMemberRole[]).includes(role)"
      :name="role === CLAN_MEMBER_ROLE.Leader ? 'crpg:clan-role-leader' : 'crpg:clan-role-officer'"
      :class="classes.icon()"
    />

    <span v-if="!hiddenLabel" :class="classes.label()">
      {{ $t(`clan.role.${role}`) }}
    </span>
  </div>
</template>
