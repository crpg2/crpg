<script setup lang="ts">
import type { Battle } from '~/models/strategus/battle'

import { useUser } from '~/composables/user/use-user'
import { battleIconByType } from '~/services/strategus/battle-service'
import { settlementIconByType } from '~/services/strategus/settlement-service'

const { battle } = defineProps<{ battle: Battle }>()
const { t } = useI18n()
const { clan } = useUser()

const battleTitle = computed(
  () => {
    if (battle.type === 'Siege' && battle.defender?.settlement) {
      return t('strategus.battle.titleByType.Siege', { settlement: battle.defender.settlement.name })
    }

    if (battle.type === 'Battle') {
      return t('strategus.battle.titleByType.Battle', {
        nearestSettlement: 'nearestSettlement', // TODO: get nearest settlement to point
        terrain: 'terrain', // TODO: terrain service get terrain at point

      })
    }

    return ''
    // return battle.defender?.party === null
    //   ? t(`strategus.battle.titleByType.title.${battle.type}`, { settlement: battle.defender.settlement?.name })
    //   : t('strategus.battle.party.title', {

    //     })
  },
)

// TODO: подсвечивать бои
const rowClass = (battle: Battle) => {
  const userClanId = clan.value?.id

  //  TODO: FIXME:
  const isClanBattle = [
    battle.attacker.party?.user?.clanMembership?.clan.id,
    battle.defender?.party?.user?.clanMembership?.clan.id,
    battle.defender?.settlement?.owner?.clanMembership?.clan.id,
  ]
    .filter(Boolean)
    .includes(userClanId)

  return isClanBattle ? 'text-primary' : 'text-content-100'
}
</script>

   <!-- :style="[
      {
        ...({
          backgroundColor: `color-mix(in srgb, #000 10%, var(--color-success) 35%)`,
        }),
      },
    ]" -->
<template>
  <UCard
    variant="subtle"
    :ui="{
      header: 'flex justify-between items-center gap-4',
    }"
  >
    <template #header>
      <div class="flex items-center gap-3">
        <UiTextView variant="p" tag="h5">
          {{ battleTitle }}
        </UiTextView>
      </div>

      <div class="flex items-center gap-3">
        <UiTextView variant="p" tag="h5">
          {{ $t(`strategus.battle.phase.${battle.phase}`) }}
        </UiTextView>
        <UBadge
          v-if="battle.scheduledFor"
          :label="$d(battle.scheduledFor, 'short')"
          variant="subtle"
          color="neutral"
        />

        <UiTextView variant="caption">
          {{ $t(`region.${battle.region}`) }} · {{ $t(`strategus.battle.type.${battle.type}`) }}
        </UiTextView>
      </div>
    </template>

    <div class="flex justify-center gap-6">
      <div class="flex flex-1 flex-col items-end gap-y-3.5 text-right">
        <div class="flex items-center justify-end gap-2">
          <UiTextView variant="caption-sm">
            {{ $t('strategus.battle.side.attacker') }}
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

      <UTooltip :text="battle.type" :content="{ side: 'top' }">
        <USeparator
          orientation="vertical"
          class="h-28 self-center"
          size="sm"
          :icon="`crpg:${battleIconByType[battle.type]}`"
          :ui="{
            icon: 'size-6',
          }"
        />
      </UTooltip>

      <div class="flex-1 space-y-3.5 text-left">
        <div class="flex flex-row-reverse items-center justify-end gap-2">
          <UiTextView variant="caption-sm">
            {{ $t('strategus.battle.side.defender') }}
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
            <UiDataMedia
              :label="battle.defender.settlement.name"
              :icon="`crpg:${settlementIconByType[battle.defender.settlement.type]}`"
              size="md"
            />
            <template v-if="battle.defender.settlement.owner">
              <!-- <span> • </span> -->

              <UserMedia
                v-if="battle.defender.settlement.owner"
                :user="battle.defender.settlement.owner"
              />

              <!-- <USeparator orientation="vertical" class="h-8" /> -->
            </template>
          </div>
        </template>
      </div>
    </div>
  </UCard>
</template>
