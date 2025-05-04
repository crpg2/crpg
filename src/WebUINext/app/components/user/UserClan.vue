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
    class="group flex items-center gap-1 hover:opacity-75"
    :to="{ name: 'clans-id', params: { id: clan.id } }"
  >
    <UiTooltip
      v-if="clanRole && [ClanMemberRole.Leader, ClanMemberRole.Officer].includes(clanRole)"
      placement="bottom"
      :title="$t(`clan.role.${clanRole}`)"
    >
      <ClanRoleIcon
        :role="clanRole"
      />
    </UiTooltip>

    <ClanTagIcon :color="clan.primaryColor" />
    [{{ clan.tag }}]
  </NuxtLink>
</template>
