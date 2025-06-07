<script setup lang="ts">
import type { Clan } from '~/models/clan'

import { ClanMemberRole } from '~/models/clan'

const { clan, clanRole } = defineProps<{
  clan: Clan
  clanRole?: ClanMemberRole | null
}>()
</script>

<template>
  <NuxtLink
    class="inline-flex items-center gap-1 hover:text-highlighted"
    :to="{ name: 'clans-id', params: { id: clan.id } }"
  >
    <UTooltip
      v-if="clanRole && [ClanMemberRole.Leader, ClanMemberRole.Officer].includes(clanRole)"
      :text="$t(`clan.role.${clanRole}`)"
    >
      <ClanRoleIcon :role="clanRole" />
    </UTooltip>

    <ClanTagIcon :color="clan.primaryColor" />

    [{{ clan.tag }}]
  </NuxtLink>
</template>
