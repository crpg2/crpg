<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { CharacterMedia, UserMedia } from '#components'

import type { BattleMercenary } from '~/models/strategus/battle'

import { useBattle, useBattleFighters, useBattleMercenaries, useBattleMercenaryApplications } from '~/composables/strategus/battle/use-battle'
import { useUser } from '~/composables/user/use-user'
import { BATTLE_PHASE, BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType, getBattleFighterByUserId, getBattleTitle } from '~/services/strategus/battle-service'

definePageMeta({
  layoutOptions: {
    bg: 'background-3.webp',
  },
})

const { t } = useI18n()
const { user } = useUser()

const { battle } = useBattle()
const battleTitle = computed(() => getBattleTitle(battle.value))

const {
  battleFighters,
  battleFightersCount,
  loadBattleFighters,
} = useBattleFighters()

const {
  battleMercenaries,
  battleMercenariesCount,
  battleMercenariesAttackers,
  battleMercenariesDefenders,
  loadBattleMercenaries,
  loadingBattleMercenaries,
} = useBattleMercenaries()

const {
  mercenaryApplications,
  mercenaryApplicationsCount,
} = useBattleMercenaryApplications()

const selfFighter = computed(() => getBattleFighterByUserId(battleFighters.value, user.value!.id))

const mySide = computed(() => selfFighter.value?.side ?? null)

const selfMercenary = computed(() => battleMercenaries.value.find(merc => merc.user.id === user.value!.id))

const canJoinAsMercenary = computed(() =>
  selfFighter.value == null && battle.value?.phase === BATTLE_PHASE.Hiring && battleMercenaries.value.length === 0,
)

// mercinariesTable

const table = useTemplateRef('table')
const columns: TableColumn<BattleMercenary>[] = [
  {
    accessorKey: 'user',
    cell: ({ row }) => h(UserMedia, {
      user: row.original.user,
      // isSelf: checkIsSelfMember(row.original),
      hiddenClan: true,
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
  <UContainer
    class="space-y-8 py-12"
  >
    <AppPageHeaderGroup
      :title="battleTitle"
      :back-to="{ name: 'battles' }"
    />

    <div class="mx-auto max-w-lg space-y-5">
      <UiDecorSeparator />

      <div class="flex flex-wrap items-center justify-center gap-4.5">
        <BattlePhaseBadge :phase="battle.phase" />
        <UBadge icon="i-lucide-calendar-check" :label="$d(battle.scheduledFor!, 'short')" size="xl" variant="soft" color="neutral" />
        <UBadge icon="crpg:region" :label="$t(`region.${battle.region}`)" size="xl" variant="soft" color="neutral" />
      </div>

      <UiDecorSeparator />
    </div>

    <pre>
      {{ mercenaryApplications }}
    </pre>

    <div class="mx-auto max-w-lg">
      <BattleSideComparison
        :battle
        :my-side
        can-view-mercenaries
        :attacker-mercenary-count="battleMercenariesAttackers.length"
        :defender-mercenary-count="battleMercenariesDefenders.length"
      />
    </div>

    <div>
      <div class="mx-auto max-w-3xl space-y-4">
        <UTable
          ref="table"
          class="relative rounded-md border border-muted"
          :loading="loadingBattleMercenaries"
          :data="battleMercenaries"
          :columns
        >
          <!-- @select="(_, row) => openMemberDetail(row.original)" -->
          <template #empty>
            <UiResultNotFound />
          </template>
        </UTable>
      </div>
    </div>

    <div>
      <pre>
        <!-- battleMercenaries: {{ battleMercenaries }} -->
        <!-- battleFighters: {{ battleFighters }}
        battle: {{ battle }} -->
      </pre>
    </div>
  </UContainer>
</template>
