<script setup lang="ts">
import type { Battle, BattleFighter, BattleMercenary, BattleMercenaryApplicationCreation } from '~/models/strategus/battle'

import { useBattleFighters } from '~/composables/strategus/use-battle-fighters'
import { useBattleFighterApplications } from '~/composables/strategus/use-battle-fighters-applications'
import { useBattleMercenaries } from '~/composables/strategus/use-battle-mercenaries'
import { useBattleMercenaryApplications } from '~/composables/strategus/use-battle-mercenaries-applications'
import { useBattle } from '~/composables/strategus/use-battles'
import { useLanguages } from '~/composables/use-language'
import { usePagination } from '~/composables/use-pagination'
import { useRegion } from '~/composables/use-region'
import { useSearchDebounced } from '~/composables/use-search-debounce'
import { Culture } from '~/models/culture'
import { BattlePhase, BattleSide } from '~/models/strategus/battle'
import { itemCultureToIcon } from '~/services/item-service' // TODO: culture service
import { getBattleFighter, getBattles } from '~/services/strategus-service/battle-service'
import { settlementIconByType } from '~/services/strategus-service/settlement'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  id: string
}>()

const getIconByCulture = (cultureString: string) => itemCultureToIcon[Culture[cultureString as keyof typeof Culture] || Culture.Neutral]
const { battleFightersLoading, battleFighters, battleFightersCount, battleFightersAttackers, battleFightersDefenders, loadBattleFighters } = useBattleFighters()
const { battleMercenariesLoading, battleMercenaries, battleMercenariesCount, battleMercenariesAttackers, battleMercenariesDefenders, loadBattleMercenaries } = useBattleMercenaries()

definePage({
  meta: {
    bg: 'background-4.webp',
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

const { mercenaryApplicationsCount, mercenaryApplications, loadBattleMercenaryApplications } = useBattleMercenaryApplications()
const { fighterApplicationsCount, loadBattleFighterApplications } = useBattleFighterApplications()

const { battle, battleId, loadBattle } = useBattle(props.id)

const isSelfUser = (row: BattleFighter) => row.party?.user.id === userStore.user?.id
const selfFighter = computed(() => getBattleFighter(battleFighters.value, userStore.user!.id))

const hasAppliedAsMercenary = computed(() =>
  myMercenaryApplication.value != null,
)

const myApp = computed(() =>
  mercenaryApplications.value.find(app => app.user.id === userStore.user?.id),
)

const myMercenaryApplication = computed(() => {
  if (myApp.value != null) {
    return {
      characterId: myApp.value?.character.id,
      side: myApp.value?.side,
      wage: myApp.value?.wage,
      note: myApp.value?.note,
    } as Omit<BattleMercenaryApplicationCreation, 'userId'>
  }
  else { return undefined }
})

const canManageApplications = computed(() =>
  selfFighter.value !== null,
)

const canManageBattle = computed(() =>
  selfFighter.value?.commander === true,
)

const canJoinAsMercenary = computed(() =>
  selfFighter.value == null && battle.value?.phase === BattlePhase.Hiring && battleMercenaries.value.length === 0,
)

const applicationsCount = computed(() =>
  fighterApplicationsCount.value + mercenaryApplicationsCount.value,
)

const hasOptions = computed(() =>
  canManageApplications.value || canManageBattle.value || canJoinAsMercenary.value,
)

const rowClass = (row: BattleFighter): string =>
  isSelfUser(row) ? 'text-primary' : 'text-content-100'

const attackerMercenarySlots = computed(() => {
  return battleFightersAttackers.value.reduce((total, fighter) => {
    return total + (fighter.mercenarySlots || 0) // Add the value or 0 if it's undefined
  }, 0)
})

const defenderMercenarySlots = computed(() => {
  return battleFightersDefenders.value.reduce((total, fighter) => {
    return total + (fighter.mercenarySlots || 0) // Add the value or 0 if it's undefined
  }, 0)
})

const defenderCommander = computed(() => {
  return battle.value?.defender?.party ?? battle.value?.defender?.settlement?.owner
},
)

const fetchPageData = async (battleId: number) => {
  await Promise.all([loadBattle(0, { id: battleId }), loadBattleFighters(0, { id: battleId }), loadBattleMercenaries(0, { id: battleId }), loadBattleMercenaryApplications(0, { id: battleId }), loadBattleFighterApplications(0, { id: battleId })])
}

await fetchPageData(Number(props.id))
if (userStore.characters.length === 0) {
  await userStore.fetchCharacters()
}
</script>

<template>
  <div class="p-6">
    <OButton
      v-tooltip.bottom="$t('nav.back')"
      tag="router-link"
      :to="{ name: 'StrategusBattles' }"
      variant="secondary"
      size="xl"
      outlined
      rounded
      icon-left="arrow-left"
    />
    <div v-if="battle !== null" class="pb-4">
      <div class="container mb-1">
        <Heading
          class="mb-5" :title="battle.defender?.party === null ? $t('strategus.battle.settlement.title',
                                                                    {
                                                                      settlement: battle.defender?.settlement?.name,
                                                                    }) : $t('strategus.battle.party.title',
                  {
                    nearestSettlement: 'Settlementia',
                    terrain: 'Desert',
                  })"
        />
        <div class="mx-auto mb-16 flex max-w-7xl flex-row gap-x-5">
          <div class="basis-1/4 text-content-100">
            <h1 class="mb-8 text-center text-xl">
              {{ $t('strategus.battle.side.attackers') }}
            </h1>
            <div class="flex items-center gap-1 pb-4">
              <UserMedia
                :user="battle.attacker.party.user"
                hidden-platform
                size="xl"
              />
              <OIcon
                icon="trophy-cup"
                size="lg"
              />
            </div>
            <div
              v-for="fighter in battleFightersAttackers"
              :key="fighter.id"
              class="flex flex-col gap-3 pb-4 "
            >
              <div v-if="fighter.party?.user">
                <UserMedia
                  :user="fighter.party.user"
                  hidden-platform
                  size="xl"
                  :class="rowClass(fighter)"
                />
              </div>
              <div v-if="fighter.settlement?.owner">
                <UserMedia
                  :user="fighter.settlement.owner.user"
                  hidden-platform
                  size="xl"
                />
              </div>
            </div>
          </div>
          <div class="basis-1/2 justify-center">
            <div class="mb-8 flex flex-wrap items-center justify-center gap-4.5">
              <div class="flex items-center gap-1.5">
                <OIcon
                  icon="flag"
                  size="lg"
                  class="text-content-100"
                />
                <span
                  class="text-content-200"
                >
                  {{ $t(`strategus.battle.phase.${battle.phase.toLowerCase()}`) }}
                </span>
              </div>

              <div class="h-8 w-px select-none bg-border-200" />

              <template v-if="battle.scheduledFor">
                <div class="flex items-center gap-1.5">
                  <OIcon
                    icon="calendar"
                    size="lg"
                    class="text-content-100"
                  />
                  <span
                    class="text-content-200"
                    data-aq-clan-info="tag"
                  >{{ $d(battle.scheduledFor, 'date') }}</span>
                </div>

                <div class="h-8 w-px select-none bg-border-200" />

                <div class="flex items-center gap-1.5">
                  <OIcon
                    icon="clock"
                    size="lg"
                    class="text-content-100"
                  />
                  <span
                    class="text-content-200"
                    data-aq-clan-info="tag"
                  >{{ $d(battle.scheduledFor, 'time') }}</span>
                </div>

                <div class="h-8 w-px select-none bg-border-200" />
              </template>
              <div class="flex items-center gap-1.5">
                <OIcon
                  icon="region"
                  size="lg"
                  class="text-content-100"
                />
                <div
                  class="text-content-200"
                  data-aq-clan-info="region"
                >
                  EU
                </div>
              </div>

              <div class="h-8 w-px select-none bg-border-200" />

              <div class="flex items-center gap-1.5">
                <OIcon
                  icon="member"
                  size="lg"
                  class="text-content-100"
                />
                <span
                  class="text-content-200"
                >
                  {{ battleFightersCount }}
                </span>
              </div>

              <div class="h-8 w-px select-none bg-border-200" />

              <div class="flex items-center gap-1.5">
                <OIcon
                  icon="game-mode-captain"
                  size="lg"
                  class="text-content-100"
                />
                <span
                  class="text-content-200"
                >
                  {{ battleMercenariesCount }}
                </span>
              </div>

              <Divider />
            </div>

            <div v-if="hasOptions" class="mb-20 flex items-center justify-center gap-3">
              <BattleMercenaryApplicationForm
                :application="myMercenaryApplication"
                :battle="battle"
                :update="hasAppliedAsMercenary"
                @update="loadBattleMercenaryApplications(0, { id: battleId })"
              >
                <OButton
                  v-if="canJoinAsMercenary"
                  variant="primary"
                  outlined
                  size="xl"
                >
                  {{ hasAppliedAsMercenary ? $t('strategus.battle.application.update.title') : $t('strategus.battle.application.mercenary.title') }}
                </OButton>
              </BattleMercenaryApplicationForm>

              <template v-if="canManageApplications">
                <OButton
                  v-if="canManageApplications"
                  tag="router-link"
                  :to="{ name: 'StrategusBattlesIdApplications', params: { id: battleId } }"
                  variant="primary"
                  outlined
                  size="xl"
                >
                  {{ $t('strategus.battle.application.manage') }}
                  <template v-if="applicationsCount !== 0">
                    ({{ applicationsCount }})
                  </template>
                </OButton>
                <OButton
                  v-if="canManageBattle"
                  tag="router-link"
                  :to="{ name: 'StrategusBattlesIdManage', params: { id: battleId } }"
                  variant="primary"
                  outlined
                  size="xl"
                >
                  {{ $t('strategus.battle.manage.title') }}
                </OButton>
              </template>
            </div>

            <div>
              <div class="grid grid-cols-2">
                <div class="flex flex-row text-base text-white">
                  <div>
                    <ClanTagIcon
                      v-if="battle.attacker.party?.clan"
                      :color="battle.attacker.party?.clan.primaryColor"
                      size="4x"
                    />
                  </div>
                  <div class="flex grow flex-col">
                    <span v-if="battle.attacker.party?.clan">{{ battle.attacker.party?.clan.name }} </span>
                    <span v-else>{{ battle.attacker.party?.user.name }} </span>
                    <div class="text-2xs">
                      <OIcon icon="child" size="sm" />{{ battle.attackerTotalTroops }} <span class="text-base-500">({{ (battle.attackerTotalTroops / (battle.attackerTotalTroops + battle.defenderTotalTroops) * 100).toFixed(2) }} %)</span>
                    </div>
                  </div>
                </div>
                <div class="flex flex-row text-right text-base text-white">
                  <div class="flex grow flex-col">
                    <span v-if="battle.defender?.settlement?.owner?.clan">{{ battle.defender.settlement.owner.clan.name }}</span>
                    <span v-else-if="battle.defender?.settlement">{{ battle.defender?.settlement.name }}</span>
                    <span v-else>{{ battle.defender?.party?.user.name }}</span>
                    <div class="text-2xs">
                      <span class="text-base-500">({{ (battle.defenderTotalTroops / (battle.attackerTotalTroops + battle.defenderTotalTroops) * 100).toFixed(2) }} %) </span>{{ battle.defenderTotalTroops }}<OIcon icon="child" size="sm" />
                    </div>
                  </div>
                  <div>
                    <ClanTagIcon
                      v-if="battle.attacker.party?.clan"
                      :color="battle.attacker.party?.clan.primaryColor"
                      size="4x"
                    />
                  </div>
                </div>
              </div>
              <div class="my-4 h-2.5 w-full rounded-full bg-base-400">
                <div class="h-2.5 rounded-full bg-base-500" :style="{ width: (battle.attackerTotalTroops / (battle.attackerTotalTroops + battle.defenderTotalTroops) * 100).toFixed(2).concat('%') }" />
              </div>
              <div class="grid grid-cols-2">
                <div class="inline-flex flex-row gap-1.5 text-base text-white">
                  <OIcon
                    icon="game-mode-captain"
                    class="text-content-100"
                  />
                  {{ battleMercenariesAttackers.length }}
                </div>
                <div class="inline-flex flex-row-reverse gap-1.5 text-base text-white">
                  <OIcon
                    icon="game-mode-captain"
                    class="text-content-100"
                  />
                  {{ battleMercenariesDefenders.length }}
                </div>
              </div>
            </div>
          </div>
          <div class="basis-1/4 text-right text-content-100">
            <h1 class="mb-8 text-center text-xl">
              {{ $t('strategus.battle.side.defenders') }}
            </h1>
            <div v-if="defenderCommander" class="flex items-center gap-1 pb-4">
              <UserMedia
                :user="defenderCommander?.user"
                hidden-platform
                size="xl"
              />
              <OIcon
                icon="trophy-cup"
                size="lg"
              />
            </div>
            <div
              v-for="fighter in battleFightersDefenders"
              :key="fighter.id"
              class="flex flex-col gap-3 pb-4"
            >
              <div v-if="fighter.party?.user">
                <UserMedia
                  :user="fighter.party.user"
                  hidden-platform
                  size="xl"
                />
              </div>
              <div v-else-if="fighter.settlement?.owner">
                <UserMedia
                  :user="fighter.settlement.owner.user"
                  hidden-platform
                  size="xl"
                />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
