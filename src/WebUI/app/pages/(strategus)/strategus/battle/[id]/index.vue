<script setup lang="ts">
import { useClipboard } from '@vueuse/core'
import { groupBy } from 'es-toolkit'

import { usePartyState } from '~/composables/strategus/use-party'
import { BATTLE_SIDE } from '~/models/strategus/battle'

const { party } = usePartyState().value
const toast = useToast()
const { t, n } = useI18n()
const { copy } = useClipboard()
const battle = computed(() => party.targetedBattle!)

const battleCoordinates = computed(() => battle.value.position.coordinates.map(p => n(p)).join(', '))

const onPositionCopy = () => {
  copy(battleCoordinates.value)
  toast.add({
    title: t('action.copied'),
    close: false,
    color: 'success',
  })
}

const fightersBySide = computed(() => groupBy(battle.value.fighters, f => f.side))
</script>

<template>
  <div>
    <!-- TODO: name -->
    <!-- <UiHeading :title="party.targetedBattle!.phase" /> -->
    <div class="flex flex-wrap items-center justify-center gap-4.5">
      <BattlePhaseBadge :phase="battle.phase" />
      <UBadge icon="crpg:region" :label="$t(`region.${battle.region}`, 0)" size="xl" variant="soft" color="neutral" />
      <UBadge
        icon="i-lucide-locate-fixed"
        :label="battleCoordinates"
        size="xl"
        variant="soft"
        color="neutral"
        trailing-icon="crpg:copy"
        @click="onPositionCopy"
      />
    </div>

    <div class="grid grid-cols-[1fr_auto_1fr] gap-6">
      <template v-for="(fighters, side, idx) in fightersBySide" :key="side">
        <div>
          {{ side }}
          <div
            v-for="fighter in fighters"
            :key="fighter.id"
          >
            <template v-if="fighter.party">
              <UiDataMedia
                v-if="fighter.party.user.clanMembership?.clan"
                :label="fighter.party.user.clanMembership.clan.name"
                size="xl"
                :layout="side === BATTLE_SIDE.Attacker ? 'reverse' : 'normal'"
                :style="{ color: fighter.party.user.clanMembership.clan.primaryColor }"
              >
                <template #icon="{ classes: clanTagIconClasses }">
                  <ClanTagIcon
                    :color="fighter.party.user.clanMembership.clan.primaryColor"
                    :class="clanTagIconClasses()"
                  />
                </template>
              </UiDataMedia>

              <UserMedia :user="fighter.party.user" />
            </template>
          </div>
        </div>

        <USeparator
          v-if="idx === 0"
          orientation="vertical"
          class="h-28 self-center"
          size="sm"
          icon="crpg:game-mode-battle"
          :ui="{ icon: 'size-7' }"
        />
      </template>

      <!-- <div
        class="flex flex-1 flex-col gap-y-3.5"
        :class="side === BATTLE_SIDE.Attacker ? 'items-end text-right' : 'items-start text-left'"
      > -->
      <!-- <div
          class="flex items-center gap-2"
          :class="side === BATTLE_SIDE.Attacker
            ? 'flex-row justify-end' : 'flex-row-reverse justify-end'"
        >
          <slot name="topbar-prepend" />

          <UiTextView variant="caption-sm">
            {{ $t(`strategus.battle.side.${side}`) }}
          </UiTextView>

          <UTooltip text="Tickets count">
            <UBadge icon="crpg:member" :label="$n(sideInfo.totalTroops)" variant="subtle" />
          </UTooltip>
        </div> -->

      <!-- <template v-if="sideInfo.settlement">
          <template v-if="sideInfo.settlement.owner?.clanMembership?.clan">
            <UiDataMedia
              :label="sideInfo.settlement.owner.clanMembership.clan.name"
              size="xl"
            >
              <template #icon="{ classes: clanTagIconClasses }">
                <ClanTagIcon
                  :color="sideInfo.settlement.owner.clanMembership.clan.primaryColor"
                  :class="clanTagIconClasses()"
                />
              </template>
            </UiDataMedia>
          </template>

          <div class="flex items-center gap-4">
            <SettlementMedia :settlement="sideInfo.settlement" />

            <UserMedia
              v-if="sideInfo.settlement.owner"
              :user="sideInfo.settlement.owner"
            />
          </div>
        </template> -->

      <!-- <template v-else-if="sideInfo.commander.party">
          <UiDataMedia
            v-if="sideInfo.commander.party.user.clanMembership?.clan"
            :label="sideInfo.commander.party.user.clanMembership.clan.name"
            size="xl"
            :layout="side === BATTLE_SIDE.Attacker ? 'reverse' : 'normal'"
            :style="{ color: sideInfo.commander.party.user.clanMembership.clan.primaryColor }"
          >
            <template #icon="{ classes: clanTagIconClasses }">
              <ClanTagIcon
                :color="sideInfo.commander.party.user.clanMembership.clan.primaryColor"
                :class="clanTagIconClasses()"
              />
            </template>
          </UiDataMedia>

          <UserMedia :user="sideInfo.commander.party.user" />
        </template> -->
      <!-- </div> -->
    </div>

    <!-- <pre>{{ party.targetedBattle }}</pre> -->
  </div>
</template>
