<script setup lang="ts">
import {
  freeRespecializeIntervalDays,
  freeRespecializePostWindowHours,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { RespecCapability } from '~/services/character-service'

import { parseTimestamp } from '~/utils/date'

defineProps<{
  respecCapability: RespecCapability
  character: Character
}>()

defineEmits<{
  respec: []
}>()
</script>

<template>
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
</template>
