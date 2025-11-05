<script setup lang="ts">
import type { Battle } from '~/models/strategus/battle'

import { settlementIconByType } from '~/services/strategus/settlement-service'

const { battle } = defineProps<{ battle: Battle }>()
</script>

<template>
  <UCard
    variant="outline"
    :ui="{
      header: 'flex justify-between items-center gap-4',
    }"
  >
    <template #header>
      <div class="flex items-center gap-3">
        <UiTextView variant="p" tag="h5">
          {{ $t(`strategus.battle.phase.${battle.phase.toLowerCase()}`) }}
        </UiTextView>
        <UBadge
          v-if="battle.scheduledFor"
          :label="$d(battle.scheduledFor, 'short')"
          variant="subtle"
          color="neutral"
        />
      </div>

      <UiTextView variant="caption">
        Region: {{ battle.region }}
      </UiTextView>
    </template>

    <div class="flex justify-center gap-6">
      <div class="flex flex-1 flex-col items-end gap-y-3.5 text-right">
        <div class="flex items-center justify-end gap-2">
          <UiTextView variant="caption-sm">
            Attacker
          </UiTextView>
          <UBadge icon="crpg:member" :label="$n(battle.defenderTotalTroops)" variant="subtle" />
        </div>

        <template v-if="battle.attacker.party">
          <UiDataMedia
            v-if="battle.attacker.party.user.clanMembership?.clan"
            :label="battle.attacker.party.user.clanMembership.clan.name"
            size="xl"
            class="flex-row-reverse"
            :style="{ color: battle.attacker.party.user.clanMembership.clan.primaryColor }"
          >
            <template #icon="{ classes: clanTagIconClasses }">
              <ClanTagIcon
                :color="battle.attacker.party.user.clanMembership.clan.primaryColor"
                :class="clanTagIconClasses()"
              />
            </template>
          </UiDataMedia>

          <UserMedia :user="battle.attacker.party.user" />
        </template>
      </div>

      <!-- TODO: icon by battle type -->
      <USeparator
        orientation="vertical"
        class="h-28 self-center"
        size="sm"
        icon="crpg:game-mode-conquest"
        :ui="{
          icon: 'size-6',
        }"
      />

      <div class="flex-1 space-y-3.5 text-left">
        <div class="flex flex-row-reverse items-center justify-end gap-2">
          <UiTextView variant="caption-sm">
            Defender
          </UiTextView>
          <UBadge icon="crpg:member" :label="$n(battle.defenderTotalTroops)" variant="subtle" />
        </div>

        <template v-if="battle.defender?.party">
          <UiDataMedia
            v-if="battle.defender.party.user.clanMembership?.clan"
            :label="battle.defender.party.user.clanMembership.clan.name"
            size="xl"
          >
            <template #icon="{ classes: clanTagIconClasses }">
              <ClanTagIcon
                :color="battle.defender.party.user.clanMembership.clan.primaryColor"
                :class="clanTagIconClasses()"
              />
            </template>
          </UiDataMedia>

          <UserMedia :user="battle.defender.party.user" />
        </template>

        <template v-else-if="battle.defender?.settlement">
          <template v-if="battle.defender.settlement.owner">
            <UiDataMedia
              v-if="battle.defender.settlement.owner.clanMembership?.clan"
              :label="battle.defender.settlement.owner.clanMembership.clan.name"
              size="xl"
            >
              <template #icon="{ classes: clanTagIconClasses }">
                <ClanTagIcon
                  :color="battle.defender.settlement.owner.clanMembership.clan.primaryColor"
                  :class="clanTagIconClasses()"
                />
              </template>
            </UiDataMedia>
          </template>

          <div class="flex items-center gap-4">
            <!-- <UserMedia
              v-if="battle.defender.settlement.owner"
              :user="battle.defender.settlement.owner"
            /> -->

            <UiDataMedia
              :label="battle.defender.settlement.name"
              :icon="`crpg:${settlementIconByType[battle.defender.settlement.type]}`"
              size="md"
            />
          </div>
        </template>
      </div>
    </div>
  </UCard>
</template>
