<script setup lang="ts">
import type { Battle, BattleFighterApplication, BattleMercenary, BattleMercenaryApplication } from '~/models/strategus/battle'

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
import { BattleApplicationType, BattlePhase, BattleSide } from '~/models/strategus/battle'
import { notify } from '~/services/notification-service'
import { getBattleFighter, getBattles, respondToBattleFighterApplication, respondToBattleMercenaryApplication } from '~/services/strategus-service/battle-service'
import { settlementIconByType } from '~/services/strategus-service/settlement'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  id: string
}>()

definePage({
  meta: {
    layout: 'default',
    middleware: '', // TODO: ['canManageApplications']
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const applicationTypes = Object.keys(BattleApplicationType)

const route = useRoute()
const router = useRouter()

const { battle, battleId, loadBattle } = useBattle(props.id)
const { mercenaryApplicationsCount, mercenaryApplications, loadBattleMercenaryApplications } = useBattleMercenaryApplications()
const { fighterApplicationsCount, fighterApplications, loadBattleFighterApplications } = useBattleFighterApplications()
const { pageModel, perPage } = usePagination()

const applicationCountByApplicationType = (type: string) => {
  if (type === BattleApplicationType.Fighter) {
    return fighterApplicationsCount.value
  }
  else {
    return mercenaryApplicationsCount.value
  }
}

const canRecruitMercenaries = computed(() =>
  battle.value?.phase === BattlePhase.Hiring,
)

const canRecruitFighters = computed(() =>
  battle.value?.phase === BattlePhase.Preparation,
)

const respondToMercenary = async (application: BattleMercenaryApplication, status: boolean) => {
  await respondToBattleMercenaryApplication(battleId.value, application.id, status)
  await loadBattleMercenaryApplications(0, { id: battleId.value })

  status
    ? notify(t('clan.application.respond.accept.notify.success'))
    : notify(t('clan.application.respond.decline.notify.success'))
}

const respondToFighter = async (application: BattleFighterApplication, status: boolean) => {
  await respondToBattleFighterApplication(battleId.value, application.id, status)
  await loadBattleFighterApplications(0, { id: battleId.value })

  status
    ? notify(t('clan.application.respond.accept.notify.success'))
    : notify(t('clan.application.respond.decline.notify.success'))
}

// todo: conditional load?
const fetchPageData = async (battleId: number) => {
  await Promise.all([loadBattle(0, { id: battleId }), loadBattleMercenaryApplications(0, { id: battleId }), loadBattleFighterApplications(0, { id: battleId })])
}

await fetchPageData(Number(props.id))
</script>

<template>
  <div class="p-6">
    <OButton
      v-tooltip.bottom="$t('nav.back')"
      tag="router-link"
      :to="{ name: 'StrategusBattlesId', params: { id: battleId } }"
      variant="secondary"
      size="xl"
      outlined
      rounded
      icon-left="arrow-left"
    />
    <div class="mx-auto max-w-4xl py-6">
      <h1 class="mb-14 text-center text-xl text-content-100">
        {{ $t('strategus.battle.application.page.title') }}
      </h1>

      <div class="container">
        <div class="mx-auto max-w-4xl">
          <OTable
            v-if="canRecruitFighters"
            v-model:current-page="pageModel"
            :data="fighterApplications"
            :per-page="perPage"
            bordered
            :paginated="fighterApplications.length > perPage"
          >
            <OTableColumn
              v-slot="{ row: application }: { row: BattleFighterApplication }"
              field="name"
              :label="$t('strategus.battle.application.table.column.name')"
            >
              <UserMedia
                :user="application.party.user"
                hidden-clan
              />
            </OTableColumn>

            <OTableColumn
              v-slot="{ row: application }: { row: BattleFighterApplication }"
              field="party"
              :label="$t('strategus.battle.application.table.column.troops')"
            >
              {{ application.party.troops }}
            </OTableColumn>

            <OTableColumn
              v-slot="{ row: application }: { row: BattleFighterApplication }"
              field="action"
              position="right"
              :label="$t('strategus.battle.application.table.column.actions')"
              width="160"
            >
              <div class="flex items-center justify-center gap-1">
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.accept')"
                  size="xs"
                  @click="respondToFighter(application, true)"
                />
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.decline')"
                  size="xs"
                  @click="respondToFighter(application, false)"
                />
              </div>
            </OTableColumn>

            <template #empty>
              <ResultNotFound />
            </template>
          </OTable>
          <OTable
            v-if="canRecruitMercenaries"
            v-model:current-page="pageModel"
            :data="mercenaryApplications"
            :per-page="perPage"
            bordered
            :paginated="mercenaryApplications.length > perPage"
          >
            <OTableColumn
              v-slot="{ row: application }: { row: BattleMercenaryApplication }"
              field="name"
              :label="$t('strategus.battle.application.table.column.name')"
            >
              <UserMedia
                :user="application.user"
                hidden-clan
              />
            </OTableColumn>

            <OTableColumn
              v-slot="{ row: application }: { row: BattleMercenaryApplication }"
              field="wage"
              :label="$t('strategus.battle.application.table.column.wage')"
            >
              <Coin :value="application.wage" />
            </OTableColumn>

            <OTableColumn
              v-slot="{ row: application }: { row: BattleMercenaryApplication }"
              field="note"
              :label="$t('strategus.battle.application.table.column.note')"
            >
              {{ application.note }}
            </OTableColumn>

            <OTableColumn
              v-slot="{ row: application }: { row: BattleMercenaryApplication }"
              field="action"
              position="right"
              :label="$t('strategus.battle.application.table.column.actions')"
              width="160"
            >
              <div class="flex items-center justify-center gap-1">
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.accept')"
                  size="xs"
                  @click="respondToMercenary(application, true)"
                />
                <OButton
                  variant="primary"
                  inverted
                  :label="$t('action.decline')"
                  size="xs"
                  @click="respondToMercenary(application, false)"
                />
              </div>
            </OTableColumn>

            <template #empty>
              <ResultNotFound />
            </template>
          </OTable>
        </div>
      </div>
    </div>
  </div>
</template>
