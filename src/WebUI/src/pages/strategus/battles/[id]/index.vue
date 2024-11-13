<script setup lang="ts">
import type { BattleMercenaryApplicationCreation } from '~/models/strategus/battle'

import BattleSideComparison from '~/components/strategus/battle/BattleSideComparison.vue'
import { useBattleFighters } from '~/composables/strategus/use-battle-fighters'
import { useBattleFighterApplications } from '~/composables/strategus/use-battle-fighters-applications'
import { useBattleMercenaries } from '~/composables/strategus/use-battle-mercenaries'
import { useBattleMercenaryApplications } from '~/composables/strategus/use-battle-mercenaries-applications'
import { useBattle } from '~/composables/strategus/use-battles'
import { BattlePhase, BattleSide } from '~/models/strategus/battle'
import { notify } from '~/services/notification-service'
import { getBattleFighter, getBattles, removeBattleMercenary } from '~/services/strategus-service/battle-service'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  id: string
}>()

const { battleFighters, battleFightersCount, loadBattleFighters } = useBattleFighters()
const { battleMercenaries, battleMercenariesCount, battleMercenariesAttackers, battleMercenariesDefenders, loadBattleMercenaries } = useBattleMercenaries()

definePage({
  meta: {
    bg: 'background-4.webp',
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const userStore = useUserStore()

const { mercenaryApplicationsCount, mercenaryApplications, loadBattleMercenaryApplications } = useBattleMercenaryApplications()
const { fighterApplicationsCount, loadBattleFighterApplications } = useBattleFighterApplications()

const { battle, battleId, loadBattle } = useBattle(props.id)

const selfFighter = computed(() => getBattleFighter(battleFighters.value, userStore.user!.id))
const selfMercenary = computed(() => battleMercenaries.value.find(merc => merc.user.id === userStore.user!.id))

const hasAppliedAsMercenary = computed(() =>
  myMercenaryApplication.value != null,
)

const RemoveBattleMercenary = async (battleId: number, mercenaryId: number) => {
  await removeBattleMercenary(battleId, mercenaryId)
  notify(t('strategus.battle.mercenary.remove.notify.success'))
  await fetchPageData(Number(props.id))
}

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

const mySide = computed (() => {
  if (selfFighter.value) {
    return selfFighter.value.side
  }

  return null
})

const canManageApplications = computed(() =>
  selfFighter.value !== null,
)

const canManageBattle = computed(() =>
  selfFighter.value?.commander === true,
)

const canViewMercenaries = computed (() =>
  battle.value?.phase !== BattlePhase.Hiring && battle.value?.phase !== BattlePhase.Preparation,
)

const canJoinAsMercenary = computed(() =>
  selfFighter.value == null && battle.value?.phase === BattlePhase.Hiring && battleMercenaries.value.length === 0,
)

const applicationsCount = computed(() =>
  fighterApplicationsCount.value + mercenaryApplicationsCount.value,
)

const hasOptions = computed(() =>
  canManageApplications.value || canManageBattle.value || canJoinAsMercenary.value || selfMercenary.value,
)

const fetchPageData = async (battleId: number) => {
  await Promise.all([loadBattle(0, { id: battleId }), loadBattleFighters(0, { id: battleId })])
  if (battle.BattlePhase !== BattlePhase.Preparation) {
    await Promise.all([loadBattleMercenaries(0, { id: battleId }), loadBattleMercenaryApplications(0, { id: battleId })])
  }
  else {
    await Promise.all([loadBattleFighterApplications(0, { id: battleId })])
  }
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
                    nearestSettlement: 'nearestSettlement', // TODO: get nearest settlement to point
                    terrain: 'terrain', // TODO: terrain service get terrain at point
                  })"
        />
        <div class="mx-auto mb-16 flex max-w-7xl flex-row gap-x-5">
          <div class="basis-1/4">
            <BattleSideFighters :battle="battle" :fighters="battleFighters" :side="BattleSide.Attacker" />
          </div>
          <div class="basis-1/2 justify-center">
            <div class="mb-8 flex flex-wrap items-center justify-center gap-4.5">
              <div class="flex items-center gap-1.5">
                <OIcon
                  v-tooltip.bottom="$t('strategus.battle.tooltip.phase')"
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
                    v-tooltip.bottom="$t('strategus.battle.tooltip.date')"
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
                    v-tooltip.bottom="$t('strategus.battle.tooltip.time')"
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
                  v-tooltip.bottom="$t('strategus.battle.tooltip.region')"
                  icon="region"
                  size="lg"
                  class="text-content-100"
                />
                <div
                  class="text-content-200"
                >
                  EU
                </div>
              </div>

              <div class="h-8 w-px select-none bg-border-200" />

              <div class="flex items-center gap-1.5">
                <OIcon
                  v-tooltip.bottom="$t('strategus.battle.tooltip.fighters')"
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
                  v-tooltip.bottom="$t('strategus.battle.tooltip.mercenaries')"
                  icon="game-mode-captain"
                  size="lg"
                  class="text-content-100"
                />
                <span
                  class="text-content-200"
                >
                  {{ canViewMercenaries ? battleMercenariesCount : '?' }}
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
              <ConfirmActionTooltip
                v-if="selfMercenary"
                :confirm-label="$t('action.ok')"
                :title="$t('strategus.battle.mercenary.remove.confirm')"
                placement="bottom"
                @confirm="RemoveBattleMercenary(battleId, selfMercenary.id)"
              >
                <OButton
                  v-if="selfMercenary"
                  variant="primary"
                  outlined
                  size="xl"
                >
                  {{ $t('strategus.battle.application.mercenary.resign') }}
                </OButton>
              </ConfirmActionTooltip>
              <template v-if="canManageApplications">
                <OButton
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
            <BattleSideComparison
              :battle :my-side
              :can-view-mercenaries
              :attacker-mercenary-count="battleMercenariesAttackers.length"
              :defender-mercenary-count="battleMercenariesDefenders.length"
            />
          </div>
          <div class="basis-1/4 text-right">
            <BattleSideFighters :battle="battle" :fighters="battleFighters" :side="BattleSide.Defender" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
