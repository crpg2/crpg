<script setup lang="ts">
import { ClanMemberRole } from '~/models/clan'

defineProps<{
  role: ClanMemberRole
}>()
</script>

<template>
  <UIcon
    :name="role === ClanMemberRole.Leader ? 'crpg:clan-role-leader' : 'crpg:clan-role-officer'"
    class="size-5"
    :class="
      [
        role === ClanMemberRole.Leader
          ? 'text-more-support'
          : role === ClanMemberRole.Officer
            ? 'text-content-100'
            : 'text-content-400',
      ]
    "
  />
</template>
