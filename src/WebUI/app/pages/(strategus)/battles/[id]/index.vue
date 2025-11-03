<script setup lang="ts">
import { useBattle, useBattleFighters, useBattleMercenaries } from '~/composables/strategus/battle/use-battle'

const { battle } = useBattle()
const { t } = useI18n()

const pageTitle = computed(
  () => {
    return battle.value.defender?.party === null
      ? t('strategus.battle.settlement.title', { settlement: battle.value.defender.settlement?.name })
      : t('strategus.battle.party.title', {
          nearestSettlement: 'nearestSettlement', // TODO: get nearest settlement to point
          terrain: 'terrain', // TODO: terrain service get terrain at point
        })
  },
)

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
} = useBattleMercenaries()
</script>

<template>
  <UContainer
    class="space-y-8 py-12"
  >
    <UiHeading
      :title="pageTitle"
    >
      <!-- <template #icon>
        <ClanTagIcon
          :color="clan.primaryColor"
          class="size-12"
        />
      </template> -->
    </UiHeading>

    <div class="mx-auto max-w-lg space-y-5">
      <UiDecorSeparator />

      <div class="flex flex-wrap items-center justify-center gap-4.5">
        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:hash" class="size-6" />
          </template>
          <span>{{ $d(battle.scheduledFor!, 'short') }}</span>
        </UiDataCell>

        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:region" class="size-6" />
          </template>
          <span>{{ battle.region }}</span>
        </UiDataCell>

        <!-- <USeparator orientation="vertical" class="h-8" />

        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:region" class="size-6" />
          </template>
          <span data-aq-clan-info="region"> {{ $t(`region.${clan.region}`, 0) }}</span>
          <template #rightContent>
            <div class="flex items-center gap-1">
              <UTooltip
                v-for="l in clan.languages"
                :key="l"
                :text="$t(`language.${l}`)"
              >
                <UBadge
                  :label="l"
                  color="primary"
                  variant="subtle"
                />
              </UTooltip>
            </div>
          </template>
        </UiDataCell>

        <USeparator orientation="vertical" class="h-8" />

        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:member" class="size-6" />
          </template>
          <span data-aq-clan-info="member-count">{{ clanMembersCount }}</span>
        </UiDataCell> -->
      </div>

      <!-- <UiTextView
        v-if="clan.description"
        variant="p"
        class="mt-7 text-center"
        data-aq-clan-info="description"
      >
        {{ clan.description }}
      </UiTextView> -->

      <UiDecorSeparator />
    </div>

    <div>
      {{ battle }}
    </div>
  </UContainer>
</template>
