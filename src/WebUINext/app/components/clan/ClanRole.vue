<script setup lang="ts">
import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'
import type { ClanMemberRole } from '~/models/clan'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

const { hiddenLabel = false, size = 'md' } = defineProps<{
  hiddenLabel?: boolean
  role: ClanMemberRole
  size?: DataMediaSize
}>()
</script>

<template>
  <UiDataMedia
    :size
    :class="
      role === CLAN_MEMBER_ROLE.Leader
        ? 'text-[var(--color-gold)]'
        : role === CLAN_MEMBER_ROLE.Officer
          ? 'text-highlighted'
          : 'text-toned'
    "
    :label="!hiddenLabel ? $t(`clan.role.${role}`) : undefined"
  >
    <template
      v-if="([CLAN_MEMBER_ROLE.Leader, CLAN_MEMBER_ROLE.Officer] as ClanMemberRole[]).includes(role)"
      #icon="{ classes }"
    >
      <UIcon
        :name="role === CLAN_MEMBER_ROLE.Leader ? 'crpg:clan-role-leader' : 'crpg:clan-role-officer'"
        :class="classes()"
      />
    </template>
  </UiDataMedia>
</template>
