<script setup lang="ts">
import type { Battle, BattleFighter } from '~/models/strategus/battle'

import { useBattles } from '~/composables/strategus/battle/use-battles'
import { usePagination } from '~/composables/use-pagination'
import { useRegion } from '~/composables/use-region'
import { useSearchDebounced } from '~/composables/use-search-debounce'
import { Culture } from '~/models/culture'
import { itemCultureToIcon } from '~/services/item-service' // TODO: culture service
import { getBattles } from '~/services/strategus-service/battle'
import { settlementIconByType } from '~/services/strategus-service/settlement'
import { useUserStore } from '~/stores/user'

definePage({
  meta: {
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
})

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
)

watch(regionModel, () => loadBattles())

const filteredBattles = computed(() => {
  const searchQuery = searchModel.value.toLowerCase()

  //   TODO: FIXME:
  return battles.value.filter((battle) => {
    const getSearchableFields = (entity: BattleFighter | null) => [
      entity?.party?.user?.name,
      entity?.party?.user?.clanMembership?.clan.name,
      entity?.party?.user?.clanMembership?.clan.tag,
      entity?.settlement?.name,
      entity?.settlement?.owner?.name,
      entity?.settlement?.owner?.clanMembership?.clan.name,
      entity?.settlement?.owner?.clanMembership?.clan.tag,
    ]

    const searchableFields = [
      ...getSearchableFields(battle.attacker),
      ...getSearchableFields(battle.defender),
    ]

    return searchableFields.some(field => field?.toLowerCase().includes(searchQuery))
  })
})

const getIconByCulture = (cultureString: string) => itemCultureToIcon[Culture[cultureString as keyof typeof Culture] || Culture.Neutral]

const rowClass = (battle: Battle) => {
  const userClanId = userStore.clan?.id

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

// TODO:
const onClickRow = (battle: Battle) =>
  router.push({ name: 'StrategusBattlesId', params: { id: battle.id } })
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-6xl py-8 md:py-16">
      <Heading :title="$t('strategus.battle.title')" />

      <div class="mb-6 flex flex-wrap items-center justify-between gap-4">
        <OTabs v-model="regionModel" content-class="hidden">
          <OTabItem
            v-for="region in regions" :key="region"
            :label="$t(`region.${region}`, 0)"
            :value="region"
          />
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
            />
          </div>
        </div>
      </div>

      <OTable
        v-model:current-page="pageModel"
        :data="filteredBattles"
        :per-page="perPage"
        :paginated="battles.length > perPage"
        hoverable
        bordered
        sort-icon="chevron-up"
        sort-icon-size="xs"
        :default-sort="['scheduledFor']"
        :row-class="(row) => rowClass(row as Battle)"
        @click="(row) => onClickRow(row as Battle)"
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
          {{ $t(`strategus.battle.phase.${battle.phase.toLowerCase()}`) }}
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
            <UserMedia :user="battle.attacker.party!.user" hidden-platform />
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
          <!-- TODO: To component -->
          <!--  -->
          <div v-if="battle.defender?.party">
            <div v-if="battle.defender.party.clan">
              {{ battle.defender.party.clan.name }}
            </div>
            <UserMedia :user="battle.defender!.party!.user" hidden-platform />
          </div>

          <div v-else-if="battle.defender?.settlement">
            <div class="flex flex-row gap-x-1">
              <div v-if="!battle.defender.settlement.owner">
                <SvgSpriteImg :name="getIconByCulture(battle.defender.settlement.culture) ?? 'culture-neutrals'" viewBox="0 0 18 18" class="w-4" />
              </div>
              <div>{{ battle.defender?.settlement?.name }}</div>
              <div><OIcon v-tooltip="$t(`strategus.settlementType.${battle.defender.settlement.type}`)" :icon="settlementIconByType[battle.defender.settlement.type].icon" class="self-baseline" /></div>
            </div>

            <div v-if="battle.defender.settlement.owner">
              <UserMedia :user="battle.defender.settlement.owner" hidden-platform />
            </div>
          </div>

          <!-- TODO: -->
          {{ battle.defenderTotalTroops }}
          <OIcon icon="child" size="sm" />
        </OTableColumn>

        <template #empty>
          <ResultNotFound />
        </template>
      </OTable>
    </div>
  </div>
</template>
