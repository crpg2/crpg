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
import { getBattles, removeBattleMercenary, respondToBattleFighterApplication, respondToBattleMercenaryApplication } from '~/services/strategus-service/battle-service'
import { settlementIconByType } from '~/services/strategus-service/settlement'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'

const props = defineProps<{
  id: string
}>()

definePage({
  meta: {
    layout: 'default',
    middleware: '', // TODO: ['canManageBattle']
    roles: ['User', 'Moderator', 'Admin'],
  },
  props: true,
})

const userStore = useUserStore()
const router = useRouter()
const { battleMercenariesLoading, battleMercenaries, battleMercenariesCount, battleMercenariesAttackers, battleMercenariesDefenders, loadBattleMercenaries } = useBattleMercenaries()
const { battle, battleId, loadBattle } = useBattle(props.id)
const { pageModel, perPage } = usePagination()

const searchModel = ref<string>('')
const filteredBattleMercenaries = computed(() =>
  battleMercenaries.value.filter(mercenary =>
    mercenary.user.name.toLowerCase().includes(searchModel.value.toLowerCase()),
  ),
)

const RemoveBattleMercenary = async (battleId: number, mercenaryId: number) => {
  await removeBattleMercenary(battleId, mercenaryId)
  notify(t('strategus.battle.mercenary.remove.notify.success'))
  loadBattleMercenaries(0, { id: battleId })
}

const fetchPageData = async (battleId: number) => {
  await Promise.all([loadBattle(0, { id: battleId }), loadBattleMercenaries(0, { id: battleId })])
}

await fetchPageData(Number(props.id))
</script>

<template>
  <div class="p-6">
    <RouterLink :to="{ name: 'StrategusBattlesId', params: { id: battleId } }">
      <OButton
        v-tooltip.bottom="$t('nav.back')"
        variant="secondary"
        size="xl"
        outlined
        rounded
        icon-left="arrow-left"
      />
    </RouterLink>

    <div class="mx-auto max-w-2xl space-y-10 py-6">
      <div class="space-y-14">
        <h1 class="text-center text-xl text-content-100">
          {{ $t('strategus.battle.manage.title') }}
        </h1>
        Battle settings:
        Auto accept?
        Accept if wage?
        Retreat?
        Abort?
        Manage gear / loadout for troops?
        <div class="mx-auto max-w-4xl">
          <h1 class="pb-4 text-center text-lg">
            {{ $t('strategus.battle.mercenary.title') }}
          </h1>
          <OTable
            v-model:current-page="pageModel"
            :data="filteredBattleMercenaries"
            :per-page="perPage"
            bordered
            :paginated="battleMercenaries.length > perPage"
          >
            <OTableColumn field="user.name">
              <template #header>
                <div class="w-44">
                  <OInput
                    v-model="searchModel"
                    type="text"
                    expanded
                    clearable
                    :placeholder="$t('clan.table.column.name')"
                    icon="search"
                    rounded
                    size="xs"
                  />
                </div>
              </template>
              <template #default="{ row: mercenary }: { row: BattleMercenary }">
                <UserMedia
                  :user="mercenary.user"
                  hidden-clan
                />
              </template>
            </OTableColumn>

            <OTableColumn
              v-slot="{ row: mercenary }: { row: BattleMercenary }"
              field="action"
              position="right"
              :label="$t('strategus.battle.application.table.column.actions')"
              width="160"
            >
              <div class="items-center justify-center">
                <ConfirmActionTooltip
                  :confirm-label="$t('action.ok')"
                  :title="$t('strategus.battle.mercenary.remove.confirm')"
                  placement="bottom"
                  @confirm="RemoveBattleMercenary(battleId, mercenary.id)"
                >
                  <OButton
                    variant="primary"
                    inverted
                    :label="$t('action.remove')"
                    size="xs"
                  />
                </ConfirmActionTooltip>
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
