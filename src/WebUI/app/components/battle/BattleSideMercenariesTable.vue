<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { CharacterMedia, UserMedia } from '#components'

import type { BattleMercenary } from '~/models/strategus/battle'

defineProps<{
  battleMercenaries: BattleMercenary[]
  loading: boolean
}>()

const table = useTemplateRef('table')
const columns: TableColumn<BattleMercenary>[] = [
  {
    accessorKey: 'user',
    cell: ({ row }) => h(UserMedia, {
      user: row.original.user,
      // isSelf: checkIsSelfMember(row.original),
      // hiddenClan: true,
    }),
  },
  {
    accessorKey: 'character',
    cell: ({ row }) => h(CharacterMedia, {
      character: row.original.character,
    }),
  },

  // TODO: только для phase END
  {
    header: 'K',
    cell: () => '12',
  },
  {
    header: 'D',
    cell: () => '7',
  },
  {
    header: 'A',
    cell: () => '11',
  },
]
</script>

<template>
  <div>
    <!-- <UButton
      label="Manage"
      icon="crpg:settings"
      @click="battleManageDrawer.open({
        side,
        sideInfo: side === 'Attacker' ? battle.attacker : battle.defender,
        userId: user!.id,
        battleId: battle.id,
      })"
    /> -->

    <UTable
      ref="table"
      class="relative rounded-md border border-muted"
      :loading
      :data="battleMercenaries"
      :columns
    >
      <!-- <template #empty>
        <UiResultNotFound message="TODO:" />
      </template> -->
    </UTable>
  </div>
</template>
