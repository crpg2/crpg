<script setup lang="ts">
import { useBattles } from '~/composables/strategus/use-battles'
import { useLanguages } from '~/composables/use-language'
import { usePagination } from '~/composables/use-pagination'
import { useRegion } from '~/composables/use-region'
import { useSearchDebounced } from '~/composables/use-search-debounce'
import { Culture } from '~/models/culture'
import { type Battle, BattlePhase } from '~/models/strategus/battle'
import { itemCultureToIcon } from '~/services/item-service' // TODO: culture service
import { getBattles } from '~/services/strategus-service/battle-service'
import { settlementIconByType } from '~/services/strategus-service/settlement'
import { useUserStore } from '~/stores/user'

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()
const { battlePhaseModel } = useBattles()
const { pageModel, perPage } = usePagination()
const { searchModel } = useSearchDebounced()
const { regionModel, regions } = useRegion()

const {
  state: battles,
  execute: loadBattles,
} = useAsyncState(
  () => getBattles(regionModel.value, battlePhaseModel.value),
  [],
  {
    immediate: false,
  },
)

watch(
  () => route.query,
  async () => {
    await loadBattles()
  },
)

const getIconByCulture = (cultureString: string) => itemCultureToIcon[Culture[cultureString as keyof typeof Culture] || Culture.Neutral]

const rowClass = (battle: Battle) => {
  const userClanId = userStore.clan?.id

  const isClanBattle
    = userClanId === battle.attacker?.party?.user?.clan?.id
    || userClanId === battle.defender?.party?.user?.clan?.id
    || userClanId === battle.defender?.settlement?.owner?.user?.clan?.id

  return isClanBattle ? 'text-primary' : 'text-content-100'
}

const onClickRow = (battle: Battle) =>
  router.push({ name: 'StrategusBattlesId', params: { id: battle.id } })

await loadBattles()
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-6xl py-8 md:py-16">
      <div class="mb-6 flex flex-wrap items-center justify-between gap-4">
        <OTabs v-model="regionModel" content-class="hidden">
          <OTabItem v-for="region in regions" :label="$t(`region.${region}`, 0)" :value="region" />
        </OTabs>

        <div class="flex items-center gap-2">
          <div class="w-44">
            <OInput
              v-model="searchModel"
              type="text"
              expanded
              clearable
              :placeholder="$t('action.search')"
              icon="search"
              rounded
              size="sm"
              data-aq-search-clan-input
            />
          </div>
        </div>
      </div>

      <OTable
        v-model:current-page="pageModel"
        :data="battles"
        :per-page="perPage"
        :paginated="battles.length > perPage"
        hoverable
        bordered
        sort-icon="chevron-up"
        sort-icon-size="xs"
        :default-sort="['scheduledFor']"
        :row-class="rowClass"
        @click="onClickRow"
      >
        <OTableColumn
          v-slot="{ row: battle }: { row: Battle }"
          field="battle.phase"
          :label="$t('strategus.battle.table.column.when')"
          :width="15"
          sortable
        >
          <div v-if="battle.scheduledFor">
            {{ $d(battle.scheduledFor, 'short') }}
          </div>
        </OTableColumn>

        <OTableColumn
          v-slot="{ row: battle }: { row: Battle }"
          field="battle.phase"
          :label="$t('strategus.battle.table.column.phase')"
          :width="30"
        >
          {{ $t(`strategus.battle.phase.${battle.phase}`) }}
        </OTableColumn>

        <OTableColumn
          v-slot="{ row: battle }: { row: Battle }"
          field="battle.attacker"
          :label="$t('strategus.battle.table.column.attacker')"
          :width="120"
        >
          <div v-if="battle.attacker.party?.clan">
            {{ battle.attacker.party?.clan.name }}
          </div>
          <div>
            <UserMedia :user="battle.attacker!.party!.user" hidden-platform class="max-w-[20rem]" />
          </div>

          {{ battle.attackerTotalTroops }}
          <OIcon icon="child" size="sm" />
        </OTableColumn>

        <OTableColumn
          v-slot="{ row: battle }: { row: Battle }"
          field="battle.defender"
          :label="$t('strategus.battle.table.column.defender')"
          :width="120"
        >
          <div>
            <div v-if="battle.defender?.party">
              <div v-if="battle.defender.party.clan">
                {{ battle.defender!.party!.clan.name }}
              </div>
              <UserMedia :user="battle.defender!.party!.user" hidden-platform class="max-w-[20rem]" />
            </div>

            <div v-else-if="battle.defender?.settlement">
              <div class="flex flex-row gap-x-1">
                <div v-if="!battle.defender.settlement!.owner">
                  <SvgSpriteImg :name="getIconByCulture(battle.defender!.settlement!.culture) ?? 'culture-neutrals'" viewBox="0 0 18 18" class="w-4" />
                </div>
                <div>{{ battle.defender?.settlement?.name }}</div>
                <div><OIcon v-tooltip="$t(`strategus.settlementType.${battle.defender!.settlement.type}`)" :icon="settlementIconByType[battle.defender!.settlement.type].icon" class="self-baseline" /></div>
              </div>
              <div v-if="battle.defender.settlement.owner">
                <UserMedia :user="battle.defender!.settlement.owner!.user" hidden-platform class="max-w-[20rem]" />
              </div>
            </div>

            {{ battle.defenderTotalTroops }}
            <OIcon icon="child" size="sm" />
          </div>
        </OTableColumn>
        <template #empty>
          <ResultNotFound />
        </template>
      </OTable>
    </div>
  </div>
</template>
