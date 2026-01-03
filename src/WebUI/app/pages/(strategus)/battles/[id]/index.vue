<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { CharacterMedia, UserMedia } from '#components'

import type { BattleMercenary } from '~/models/strategus/battle'

import { useBattle, useBattleFighters, useBattleMercenaries, useBattleMercenaryApplications } from '~/composables/strategus/battle/use-battle'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_PHASE } from '~/models/strategus/battle'
import { getBattleFighterByUserId } from '~/services/strategus/battle-service'

// const { t } = useI18n()
// const { user } = useUser()

// const { battle } = useBattle()

// const {
//   battleFighters,
//   battleFightersCount,
//   loadBattleFighters,
// } = useBattleFighters()

const {
  // battleMercenaries,
  battleMercenariesBySide,
  battleMercenariesCount,
  battleMercenariesAttackers,
  battleMercenariesDefenders,
  loadBattleMercenaries,
  loadingBattleMercenaries,
} = useBattleMercenaries()

// const selfFighter = computed(() => getBattleFighterByUserId(battleFighters.value, user.value!.id))

// const mySide = computed(() => selfFighter.value?.side ?? null)

// const selfMercenary = computed(() => battleMercenaries.value.find(merc => merc.user.id === user.value!.id))

// const canJoinAsMercenary = computed(() =>
//   selfFighter.value == null && battle.value?.phase === BATTLE_PHASE.Hiring && battleMercenaries.value.length === 0,
// )

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
]
</script>

<template>
  <div class="mx-auto max-w-3xl">
    <!-- <UiTextView variant="h3" class="mb-6 text-center">
      Mercenaries
    </UiTextView> -->

    <div class="grid grid-cols-2 gap-20">
      <div v-for="(battleMercenaries, side) in battleMercenariesBySide" :key="side">
        <UTable
          ref="table"
          class="relative rounded-md border border-muted"
          :loading="loadingBattleMercenaries"
          :data="battleMercenaries"
          :columns
        >
          <template #empty>
            <UiResultNotFound />
          </template>
        </UTable>
      </div>
    </div>
  </div>
</template>
