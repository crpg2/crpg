<script setup lang="ts">
import type { BattleFighterApplication, BattleMercenaryApplication } from '~/models/strategus/battle'

import { useBattleFighterApplications } from '~/composables/strategus/battle/use-battle-fighters-applications'
import { useBattleMercenaryApplications } from '~/composables/strategus/battle/use-battle-mercenaries-applications'
import { useBattle } from '~/composables/strategus/battle/use-battles'
import { usePagination } from '~/composables/use-pagination'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { BattlePhase } from '~/models/strategus/battle'
import { notify } from '~/services/notification-service'
import { respondToBattleFighterApplication, respondToBattleMercenaryApplication } from '~/services/strategus-service/battle'
import { t } from '~/services/translate-service'

const props = defineProps<{
  id: string
}>()

definePage({
  meta: {
    middleware: '', // TODO: FIXME: ['canManageApplications']
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const battleId = Number(props.id)

const { battle, loadBattle } = useBattle()
const { mercenaryApplications, loadBattleMercenaryApplications } = useBattleMercenaryApplications()
const { fighterApplications, loadBattleFighterApplications } = useBattleFighterApplications()
const { pageModel, perPage } = usePagination()

const canRecruitMercenaries = computed(() => battle.value?.phase === BattlePhase.Hiring)
const canRecruitFighters = computed(() => battle.value?.phase === BattlePhase.Hiring)

const { execute: respondToMercenary } = useAsyncCallback(async (application: BattleMercenaryApplication, status: boolean) => {
  await respondToBattleMercenaryApplication(battleId, application.id, status)
  await loadBattleMercenaryApplications(0, { id: battleId })
  notify(status ? t('clan.application.respond.accept.notify.success') : t('clan.application.respond.decline.notify.success'))
})

const { execute: respondToFighter } = useAsyncCallback(async (application: BattleFighterApplication, status: boolean) => {
  await respondToBattleFighterApplication(battleId, application.id, status)
  await loadBattleFighterApplications(0, { id: battleId })
  notify(status ? t('clan.application.respond.accept.notify.success') : t('clan.application.respond.decline.notify.success'))
})

const fetchPageData = async (battleId: number) => {
  await loadBattle(0, { id: battleId }) // TODO: move to context

  const promises: Promise<unknown>[] = [
    ...(canRecruitMercenaries.value ? [loadBattleMercenaryApplications(0, { id: battleId })] : []),
    ...(canRecruitFighters.value ? [loadBattleFighterApplications(0, { id: battleId })] : []),
  ]

  await Promise.all(promises)
}

fetchPageData(Number(props.id))
</script>

<template>
  <div class="p-6">
    <BackButton :to="{ name: 'StrategusBattlesId', params: { id: battleId } }" />

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
              field="name"
              label="Char TODO:"
            >
              <CharacterMedia
                :character="application.character"
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
