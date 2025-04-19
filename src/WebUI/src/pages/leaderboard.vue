<script setup lang="ts">
import type { CharacterCompetitiveNumbered } from '~/models/competitive'

import { useGameMode } from '~/composables/use-game-mode'
import { useRegion } from '~/composables/use-region'
import { CharacterClass } from '~/models/character'
import { characterClassToIcon, getCompetitiveValueByGameMode } from '~/services/characters-service'
import { gameModeToIcon } from '~/services/game-mode-service'
import { createRankTable, getLeaderBoard } from '~/services/leaderboard-service'
import { useUserStore } from '~/stores/user'

definePage({
  meta: {
    bg: 'background-2.webp',
    layout: 'default',
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const route = useRoute()
const router = useRouter()

const userStore = useUserStore()

const { gameModeModel, rankedGameModes } = useGameMode()
const { regionModel, regions } = useRegion()

const characterClassModel = computed<CharacterClass | undefined>({
  get() {
    return (route.query?.class as CharacterClass) || undefined
  },

  set(characterClass: CharacterClass | undefined) {
    router.replace({
      query: {
        ...route.query,
        class: characterClass,
      },
    })
  },
})

const characterClasses = Object.values(CharacterClass)

const {
  execute: loadLeaderBoard,
  isLoading: leaderBoardLoading,
  state: leaderboard,
} = useAsyncState(
  () =>
    getLeaderBoard({
      characterClass: characterClassModel.value,
      gameMode: gameModeModel.value,
      region: regionModel.value,
    }),
  [],
)

watch(
  () => route.query,
  () => loadLeaderBoard(),
)

const rankTable = computed(() => createRankTable())

const isSelfUser = (row: CharacterCompetitiveNumbered) => row.user.id === userStore.user!.id

const rowClass = (row: CharacterCompetitiveNumbered): string =>
  isSelfUser(row) ? 'text-primary' : 'text-content-100'
</script>

<template>
  <div class="container">
    <div class="mx-auto max-w-4xl py-8 md:py-16">
      <div class="mb-20">
        <div class="mb-5 flex justify-center">
          <OIcon
            icon="trophy-cup"
            size="5x"
            class="text-more-support"
          />
        </div>
        <Heading :title="$t('leaderboard.title')" />
      </div>

      <div class="mb-4 flex items-center gap-6">
        <div class="flex items-center">
          <OTabs
            v-model="regionModel"
            content-class="hidden"
          >
            <OTabItem
              v-for="region in regions"
              :key="region"
              :label="$t(`region.${region}`, 0)"
              :value="region"
            />
          </OTabs>
        </div>
        <Divider inline />
        <div class="flex items-end">
          <OTabs
            v-model="gameModeModel"
            content-class="hidden"
          >
            <OTabItem
              v-for="gameMode in rankedGameModes"
              :key="gameMode"
              :label="$t(`game-mode.${gameMode}`, 0)"
              :icon="gameModeToIcon[gameMode]"
              :value="gameMode"
            />
          </OTabs>
        </div>
      </div>

      <OTable
        :data="leaderboard"
        hoverable
        bordered
        sort-icon="chevron-up"
        sort-icon-size="xs"
        :loading="leaderBoardLoading"
        :row-class="row => rowClass(row as CharacterCompetitiveNumbered)"
        :default-sort="['position', 'asc']"
      >
        <OTableColumn
          v-slot="{ row }: { row: CharacterCompetitiveNumbered }"
          field="position"
          :label="$t('leaderboard.table.cols.top')"
          :width="80"
          sortable
        >
          {{ row.position }}
        </OTableColumn>

        <OTableColumn
          field="rating.competitiveValue"
          :width="230"
        >
          <template #header>
            <div class="flex items-center gap-2">
              <span>{{ $t('leaderboard.table.cols.rank') }}</span>
              <Modal closable>
                <Tag
                  icon="help-circle"
                  rounded
                  size="lg"
                  variant="primary"
                />
                <template #popper>
                  <RankTable :rank-table="rankTable" />
                </template>
              </Modal>
            </div>
          </template>

          <template #default="{ row }: { row: CharacterCompetitiveNumbered }">
            <Rank
              :rank-table="rankTable"
              :competitive-value="getCompetitiveValueByGameMode(row.statistics, gameModeModel)"
            />
          </template>
        </OTableColumn>

        <OTableColumn
          v-slot="{ row }: { row: CharacterCompetitiveNumbered }"
          field="user.name"
          :label="$t('leaderboard.table.cols.player')"
        >
          <UserMedia
            class="max-w-80"
            :user="row.user"
            hidden-platform
          />
        </OTableColumn>

        <OTableColumn
          field="class"
          :width="80"
        >
          <template #header>
            <THDropdown
              :label="$t('leaderboard.table.cols.class')"
              :shown-reset="Boolean(characterClassModel)"
              @reset="characterClassModel = undefined"
            >
              <template #default="{ hide }">
                <DropdownItem
                  v-for="characterClass in characterClasses"
                  :key="characterClass"
                  :checked="characterClass === characterClassModel"
                  @click="
                    () => {
                      characterClassModel = characterClass;
                      hide();
                    }
                  "
                >
                  <OIcon
                    :icon="characterClassToIcon[characterClass]"
                    size="lg"
                  />
                  {{ $t(`character.class.${characterClass}`) }}
                </DropdownItem>
              </template>
            </THDropdown>
          </template>

          <template #default="{ row }: { row: CharacterCompetitiveNumbered }">
            <OIcon
              v-tooltip="$t(`character.class.${row.class}`)"
              :icon="characterClassToIcon[row.class]"
              size="lg"
            />
          </template>
        </OTableColumn>

        <OTableColumn
          v-slot="{ row }: { row: CharacterCompetitiveNumbered }"
          field="level"
          :label="$t('leaderboard.table.cols.level')"
          sortable
          :width="80"
        >
          {{ row.level }}
        </OTableColumn>

        <template #empty>
          <ResultNotFound />
        </template>
      </OTable>
    </div>
  </div>
</template>
