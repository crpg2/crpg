<script setup lang="ts">
import { ClanMemberRole } from '~/models/clan'

defineProps<{
  role: ClanMemberRole
}>()
</script>

<template>
  <div
    class="inline-flex items-center gap-1.5 align-middle font-bold"
    :class="
      role === ClanMemberRole.Leader
        ? 'text-more-support'
        : role === ClanMemberRole.Officer
          ? 'text-content-100'
          : 'text-content-400'
    "
  >
    <ClanRoleIcon v-if="[ClanMemberRole.Leader, ClanMemberRole.Officer].includes(role)" :role />
    {{ $t(`clan.role.${role}`) }}
  </div>
</template>
