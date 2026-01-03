<script setup lang="ts">
import { LazyBattleManageDrawer } from '#components'

import type { Battle } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { BATTLE_SIDE } from '~/models/strategus/battle'
import { battleIconByType } from '~/services/strategus/battle-service'

const { battle } = defineProps<{ battle: Battle }>()
const { user } = useUser()

const overlay = useOverlay()

const battleManageDrawer = overlay.create(LazyBattleManageDrawer)
</script>

<template>
  <div class="flex justify-center gap-6">
    <BattleSideView
      :side="BATTLE_SIDE.Attacker"
      :side-info="battle.attacker"
      :user-id="user!.id"
      @open-manage="battleManageDrawer.open({
        side: BATTLE_SIDE.Attacker,
        sideInfo: battle.attacker,
        userId: user!.id,
        battleId: battle.id,
      })"
    />

    <UTooltip :text="battle.type" :content="{ side: 'top' }">
      <USeparator
        orientation="vertical"
        class="h-28 self-center"
        size="sm"
        :icon="`crpg:${battleIconByType[battle.type]}`"
        :ui="{
          icon: 'size-7',
        }"
      />
    </UTooltip>

    <BattleSideView
      :side="BATTLE_SIDE.Defender"
      :side-info="battle.defender"
      :user-id="user!.id"
      @open-manage="battleManageDrawer.open({
        side: BATTLE_SIDE.Defender,
        sideInfo: battle.defender,
        userId: user!.id,
        battleId: battle.id,
      })"
    />
  </div>
</template>
