<script setup lang="ts">
import { tv } from 'tailwind-variants'

import type { DataMediaSize } from '~/components/ui/data/DataMedia.vue'
import type { ClanMemberRole, ClanPublic } from '~/models/clan'

import { CLAN_MEMBER_ROLE } from '~/models/clan'

const { clan, clanRole, size = 'md' } = defineProps<{
  clan: ClanPublic
  clanRole?: ClanMemberRole | null
  size?: DataMediaSize
}>()

const variants = tv({
  slots: {
    root: `
      inline-block align-middle
      hover:text-highlighted
    `,
  },

})
const classes = computed(() => variants({ size }))
</script>

<template>
  <NuxtLink
    :class="classes.root()"
    :to="{ name: 'clans-id', params: { id: clan.id } }"
  >
    <div class="flex items-center gap-0.5">
      <UTooltip :text="clan.name">
        <UiDataMedia
          :label="`[${clan.tag}]`"
          :size
        >
          <template #icon="{ classes: clanTagIconClasses }">
            <ClanTagIcon
              :color="clan.primaryColor"
              :class="clanTagIconClasses()"
            />
          </template>
        </UiDataMedia>
      </UTooltip>

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
    </div>
  </NuxtLink>
</template>
