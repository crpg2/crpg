<script setup lang="ts">
import { useToggle, useTransition } from '@vueuse/core'
import {
  experienceMultiplierByGeneration,
  maxExperienceMultiplierForGeneration,
  maximumLevel,
  minimumRetirementLevel,
} from '~root/data/constants.json'

import type {
  HeirloomPointByLevelAggregation,
} from '~/services/characters-service'

import { useCharacterRespec } from '~/composables/character/use-character-respec'
import { useGameMode } from '~/composables/use-game-mode'
import { usePollInterval } from '~/composables/use-poll-interval'
import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { GameMode } from '~/models/game-mode'
import {
  canRetireValidate,
  canSetCharacterForTournamentValidate,
  getCharacterKDARatio,
  getCharacterStatistics,
  getDefaultCharacterStatistics,
  getExperienceForLevel,
  getExperienceMultiplierBonus,
  getHeirloomPointByLevel,
  getHeirloomPointByLevelAggregation,
  retireCharacter,
  setCharacterForTournament,
  tournamentLevelThreshold,
} from '~/services/characters-service'
import { checkIsRankedGameMode, gameModeToIcon } from '~/services/game-mode-service'
import { createRankTable } from '~/services/leaderboard-service'
import { notify } from '~/services/notification-service'
import { t } from '~/services/translate-service'
import { useUserStore } from '~/stores/user'
import {
  characterCharacteristicsKey,
  characterKey,
  loadCharacterStatisticsKey,
} from '~/symbols/character'
import { msToHours } from '~/utils/date'
import { percentOf } from '~/utils/math'

definePage({
  meta: {
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const userStore = useUserStore()

const { gameModes } = useGameMode()

const character = injectStrict(characterKey)
const { loadCharacterCharacteristics } = injectStrict(characterCharacteristicsKey)

const animatedCharacterExperience = useTransition(computed(() => character.value.experience))
const currentLevelExperience = computed(() => getExperienceForLevel(character.value.level))
const nextLevelExperience = computed(() => getExperienceForLevel(character.value.level + 1))
const experiencePercentToNextLEvel = computed(() =>
  percentOf(
    character.value.experience - currentLevelExperience.value,
    nextLevelExperience.value - currentLevelExperience.value,
  ),
)

const canRetire = computed(() => canRetireValidate(character.value.level))

const { execute: onRetireCharacter, loading: retiringCharacter } = useAsyncCallback(async () => {
  userStore.replaceCharacter(await retireCharacter(character.value.id))
  await Promise.all([
    userStore.fetchUser(),
    loadCharacterCharacteristics(0, { id: character.value.id }),
  ])
  notify(t('character.settings.retire.notify.success'))
})

const [shownRetireConfirmTooltip, toggleRetireConfirmTooltip] = useToggle()

const canSetCharacterForTournament = computed(() =>
  canSetCharacterForTournamentValidate(character.value),
)

const { execute: onSetCharacterForTournament } = useAsyncCallback(async () => {
  userStore.replaceCharacter(await setCharacterForTournament(character.value.id))
  await loadCharacterCharacteristics(0, { id: character.value.id })
  notify(t('character.settings.tournament.notify.success'))
})

const { execute: loadCharacterStatistics, state: characterStatistics } = useAsyncState(
  ({ id }: { id: number }) => getCharacterStatistics(id),
  {},
  {
    immediate: false,
    resetOnExecute: false,
  },
)

const { subscribe, unsubscribe } = usePollInterval()

const rankTable = computed(() => createRankTable())

const gameMode = ref<GameMode>(GameMode.Battle)
const isRankedGameMode = computed(() => checkIsRankedGameMode(gameMode.value))

const gameModeCharacterStatistics = computed(
  () => characterStatistics.value[gameMode.value] || getDefaultCharacterStatistics(),
)

const kdaRatio = computed(() =>
  gameModeCharacterStatistics.value.deaths === 0
    ? '∞'
    : getCharacterKDARatio(gameModeCharacterStatistics.value),
)

const experienceMultiplierBonus = computed(() =>
  getExperienceMultiplierBonus(userStore.user!.experienceMultiplier),
)

const heirloomPointByLevel = computed(() => getHeirloomPointByLevel(character.value.level))
const retireTableData = computed(() => getHeirloomPointByLevelAggregation())

const { loadCharacterLimitations, respecCapability, respecializingCharacter, onRespecializeCharacter } = useCharacterRespec()

const fetchPageData = (characterId: number) =>
  Promise.all([
    loadCharacterStatistics(0, { id: characterId }),
    loadCharacterLimitations(0, { id: characterId }),
  ])

onBeforeRouteUpdate(async (to, from) => {
  if (to.name === from.name && 'id' in to.params) {
    const characterId = Number(to.params.id)
    await fetchPageData(characterId)
    unsubscribe(loadCharacterStatisticsKey)
    subscribe(loadCharacterStatisticsKey, () => {
      loadCharacterStatistics(0, { id: characterId })
    })
  }
  return true
})

await fetchPageData(character.value.id)
</script>

<template>
  <div class="mx-auto max-w-2xl space-y-12 pb-12">
    <OLoading
      full-page
      :active="respecializingCharacter || retiringCharacter"
      icon-size="xl"
    />
    <FormGroup
      :label="$t('character.settings.group.overview.title')"
      :collapsable="false"
    >
      <div class="space-y-6">
        <div class="grid grid-cols-2 gap-2 text-2xs">
          <!-- LEVEL -->
          <SimpleTableRow
            :label="$t('character.statistics.level.title')"
            :tooltip="
              character.forTournament
                ? {
                  title: $t('character.statistics.level.lockedTooltip.title', {
                    maxLevel: maximumLevel,
                  }),
                }
                : {
                  title: $t('character.statistics.level.tooltip.title', {
                    maxLevel: maximumLevel,
                  }),
                }
            "
          >
            <div
              class="flex gap-1.5"
              :class="[character.forTournament ? 'text-status-warning' : 'text-content-100']"
            >
              {{ character.level }}
              <OIcon
                v-if="character.forTournament"
                icon="lock"
                size="sm"
              />
            </div>
          </SimpleTableRow>

          <template v-if="!character.forTournament">
            <!-- GENERATION -->
            <SimpleTableRow
              :label="$t('character.statistics.generation.title')"
              :value="String(character.generation)"
              :tooltip="{
                title: $t('character.statistics.generation.tooltip.title'),
              }"
            />

            <!-- EXP. MULTI -->
            <SimpleTableRow
              :label="$t('character.statistics.expMultiplier.title')"
              :value="
                $t('character.format.expMultiplier', {
                  multiplier: $n(userStore.user!.experienceMultiplier),
                })
              "
              :tooltip="{
                title: $t('character.statistics.expMultiplier.tooltip.title', {
                  maxExpMulti: $t('character.format.expMultiplier', {
                    multiplier: $n(maxExperienceMultiplierForGeneration),
                  }),
                }),
                description: $t('character.statistics.expMultiplier.tooltip.desc'),
              }"
            />

            <div class="col-span-2 mt-12 px-4 py-2.5">
              <VueSlider
                :key="currentLevelExperience"
                class="!cursor-default !opacity-100"
                :model-value="Number(animatedCharacterExperience.toFixed(0))"
                disabled
                tooltip="always"
                :min="currentLevelExperience"
                :max="nextLevelExperience"
                :marks="[currentLevelExperience, nextLevelExperience]"
              >
                <template #mark="{ pos, value, label }">
                  <div
                    class="absolute top-2.5 whitespace-nowrap"
                    :class="{
                      '-translate-x-full': value === nextLevelExperience,
                    }"
                    :style="{ left: `${pos}%` }"
                  >
                    {{ $n(label) }}
                  </div>
                </template>
                <template #tooltip="{ value }">
                  <div
                    class="vue-slider-dot-tooltip-inner vue-slider-dot-tooltip-inner-top vue-slider-dot-tooltip-inner-disabled"
                  >
                    <div class="flex items-center">
                      <VTooltip placement="bottom">
                        <div class="flex items-center gap-1 font-semibold text-primary">
                          <OIcon
                            icon="experience"
                            size="xl"
                          />
                          {{
                            t('character.statistics.experience.format', {
                              exp: $n(value),
                              expPercent: $n(experiencePercentToNextLEvel / 100, 'percent'),
                            })
                          }}
                        </div>
                        <template #popper>
                          <div
                            class="prose prose-invert"
                            v-html="
                              $t('character.statistics.experience.tooltip', {
                                remainExpToUp: $n(nextLevelExperience - character.experience),
                              })
                            "
                          />
                        </template>
                      </VTooltip>
                    </div>
                  </div>
                </template>
              </VueSlider>
            </div>
          </template>
        </div>

        <Divider />

        <template v-if="!character.forTournament">
          <div class="flex justify-center">
            <OTabs
              v-model="gameMode"
              content-class="hidden"
              multiline
            >
              <OTabItem
                v-for="gm in gameModes"
                :key="gm"
                :label="$t(`game-mode.${gm}`, 0)"
                :icon="gameModeToIcon[gm]"
                :value="gm"
              />
            </OTabs>
          </div>

          <div class="grid grid-cols-2 gap-2 text-2xs">
            <!-- COMPETITIVE RANK -->
            <SimpleTableRow
              v-if="isRankedGameMode"
              :label="$t('character.statistics.rank.title')"
            >
              <Tooltip
                :title="$t('character.statistics.rank.tooltip.title')"
                :description="$t('character.statistics.rank.tooltip.desc')"
              >
                <Rank
                  :rank-table="rankTable"
                  :competitive-value="gameModeCharacterStatistics.rating.competitiveValue"
                />
              </Tooltip>
              <Modal closable>
                <Tag
                  icon="help-circle"
                  rounded
                  size="lg"
                  variant="primary"
                />
                <template #popper>
                  <RankTable
                    :rank-table="rankTable"
                    :competitive-value="gameModeCharacterStatistics.rating.competitiveValue"
                  />
                </template>
              </Modal>
            </SimpleTableRow>

            <!-- K/D/A -->
            <SimpleTableRow
              :label="$t('character.statistics.kda.title')"
              :value="
                $t('character.format.kda', {
                  kills: gameModeCharacterStatistics.kills,
                  deaths: gameModeCharacterStatistics.deaths,
                  assists: gameModeCharacterStatistics.assists,
                  ratio: kdaRatio,
                })
              "
              :tooltip="{
                title: $t('character.statistics.kda.tooltip.title'),
              }"
            />

            <!-- PLAY TIME -->
            <SimpleTableRow
              :label="$t('character.statistics.playTime.title')"
              :value="
                $t('dateTimeFormat.hh', { hours: msToHours(gameModeCharacterStatistics.playTime) })
              "
            />
          </div>
        </template>
      </div>
    </FormGroup>

    <FormGroup
      class="sticky bottom-0 bg-bg-main/50 backdrop-blur-sm"
      icon="settings"
      :label="$t('character.settings.group.actions.title')"
      :collapsable="false"
    >
      <div class="grid grid-cols-3 gap-4">
        <CharacterRespecButtonModal
          :respec-capability
          :character
          @respec="() => onRespecializeCharacter(character.id)"
        />

        <template v-if="!character.forTournament">
          <!--  -->
          <Modal
            @apply-hide="
              () => toggleRetireConfirmTooltip(false)
            "
          >
            <VTooltip placement="auto">
              <div>
                <OButton
                  variant="primary"
                  outlined
                  :disabled="!canRetire"
                  size="xl"
                  expanded
                  icon-left="child"
                  data-aq-character-action="retire"
                  :label="$t('character.settings.retire.title')"
                />
              </div>

              <template #popper>
                <div class="prose prose-invert">
                  <i18n-t
                    v-if="!canRetire"
                    scope="global"
                    keypath="character.settings.retire.tooltip.requiredDesc"
                    class="text-status-danger"
                    tag="p"
                  >
                    {{ $t('character.settings.retire.tooltip.required') }}
                    <template #requiredLevel>
                      <span class="font-bold">{{ minimumRetirementLevel }}+</span>
                    </template>
                  </i18n-t>

                  <h3 class="text-content-100">
                    {{ $t('character.settings.retire.tooltip.title') }}
                  </h3>

                  <i18n-t
                    scope="global"
                    keypath="character.settings.retire.tooltip.descTpl"
                  >
                    <template #desc1>
                      <i18n-t
                        scope="global"
                        keypath="character.settings.retire.tooltip.desc1"
                        tag="p"
                      >
                        <template #resetLevel>
                          <span class="font-bold text-status-danger">1</span>
                        </template>
                        <template #multiplierBonus>
                          <span class="font-bold text-status-success">
                            +{{
                              $n(experienceMultiplierByGeneration, 'percent', {
                                minimumFractionDigits: 0,
                              })
                            }}
                          </span>
                        </template>
                        <template #maxMultiplierBonus>
                          <span class="text-content-100">
                            +{{
                              $n(maxExperienceMultiplierForGeneration - 1, 'percent', {
                                minimumFractionDigits: 0,
                              })
                            }}
                          </span>
                        </template>
                      </i18n-t>
                    </template>

                    <template #desc2>
                      <i18n-t
                        scope="global"
                        keypath="character.settings.retire.tooltip.desc2"
                        tag="p"
                      >
                        <template #heirloom>
                          <OIcon
                            icon="blacksmith"
                            size="sm"
                            class="align-top text-primary"
                          />
                        </template>
                      </i18n-t>
                    </template>
                  </i18n-t>

                  <OTable
                    :data="retireTableData"
                    bordered
                    narrowed
                  >
                    <OTableColumn
                      v-slot="{ row }: { row: HeirloomPointByLevelAggregation }"
                      field="level"
                      :label="$t('character.settings.retire.loomPointsTable.cols.level')"
                    >
                      <span>{{ row.level.join(', ') }}</span>
                    </OTableColumn>
                    <OTableColumn field="points">
                      <template #header>
                        <div class="flex items-center gap-1">
                          {{ $t('character.settings.retire.loomPointsTable.cols.loomsPoints') }}
                          <OIcon
                            icon="blacksmith"
                            size="sm"
                            class="text-primary"
                          />
                        </div>
                      </template>

                      <template #default="{ row }: { row: HeirloomPointByLevelAggregation }">
                        <span>{{ row.points }}</span>
                      </template>
                    </OTableColumn>
                  </OTable>
                </div>
              </template>
            </VTooltip>

            <template #popper="{ hide }">
              <ConfirmActionForm
                :name="`${character.name} - ${character.level}`"
                :confirm-label="$t('action.apply')"
                @cancel="hide"
                @confirm="
                  () => toggleRetireConfirmTooltip(true)
                "
              >
                <template #title>
                  <div class="flex flex-col items-center gap-2">
                    <h4 class="text-xl">
                      {{ $t('character.settings.retire.dialog.title') }}
                    </h4>
                    <CharacterMedia
                      class="rounded-full border-border-300 bg-base-500/20 px-3 py-2.5 text-primary"
                      :character="character"
                      :is-active="character.id === userStore.user?.activeCharacterId"
                      :for-tournament="character.forTournament"
                    />
                  </div>
                </template>
                <template #description>
                  <p>
                    {{ $t('character.settings.retire.dialog.desc') }}
                  </p>
                  <i18n-t
                    scope="global"
                    keypath="character.settings.retire.dialog.reward"
                    tag="p"
                  >
                    <template #heirloom>
                      <span class="inline-flex items-center text-sm font-bold text-primary">
                        +{{ heirloomPointByLevel }}
                        <OIcon
                          icon="blacksmith"
                          size="sm"
                        />
                      </span>
                    </template>
                    <template #multiplierBonus>
                      <span class="text-sm font-bold text-status-success">
                        +{{
                          $n(experienceMultiplierBonus, 'percent', {
                            minimumFractionDigits: 0,
                          })
                        }}
                      </span>
                    </template>
                    <template #resetLevel>
                      <span class="text-sm font-bold text-status-danger">1</span>
                    </template>
                  </i18n-t>
                </template>
              </ConfirmActionForm>

              <ConfirmActionTooltip
                :shown="shownRetireConfirmTooltip"
                @confirm="
                  () => {
                    onRetireCharacter();
                    hide();
                  }
                "
              />
            </template>
          </Modal>

          <!--  -->
          <Modal :disabled="!canSetCharacterForTournament">
            <VTooltip placement="auto">
              <div>
                <OButton
                  variant="secondary"
                  size="xl"
                  expanded
                  icon-left="member"
                  :disabled="!canSetCharacterForTournament"
                  data-aq-character-action="forTournament"
                  :label="$t('character.settings.tournament.title')"
                />
              </div>
              <template #popper>
                <div class="prose prose-invert">
                  <h5 class="text-content-100">
                    {{ $t('character.settings.tournament.tooltip.title') }}
                  </h5>

                  <i18n-t
                    scope="global"
                    keypath="character.settings.tournament.tooltip.desc"
                    tag="p"
                  >
                    <template #tournamentLevel>
                      <span class="text-sm font-bold text-content-100">
                        {{ tournamentLevelThreshold }}
                      </span>
                    </template>
                  </i18n-t>

                  <i18n-t
                    v-if="!canSetCharacterForTournament"
                    scope="global"
                    keypath="character.settings.tournament.tooltip.requiredDesc"
                    class="text-status-danger"
                    tag="p"
                  >
                    <template #requiredLevel>
                      <span class="text-xs font-bold">{{ `<${tournamentLevelThreshold}` }}</span>
                    </template>
                  </i18n-t>
                </div>
              </template>
            </VTooltip>

            <template #popper="{ hide }">
              <ConfirmActionForm
                :title="$t('character.settings.tournament.dialog.title')"
                :description="$t('character.settings.tournament.dialog.desc')"
                :name="character.name"
                :confirm-label="$t('action.apply')"
                @cancel="hide"
                @confirm="
                  () => {
                    onSetCharacterForTournament();
                    hide();
                  }
                "
              />
            </template>
          </Modal>
        </template>
      </div>
    </FormGroup>
  </div>
</template>
