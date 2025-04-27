<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import {
  experienceMultiplierByGeneration,
  maxExperienceMultiplierForGeneration,
  minimumRetirementLevel,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { HeirloomPointByLevelAggregation } from '~/services/character-service'

import { canRetireValidate, getExperienceMultiplierBonus, getHeirloomPointByLevel, getHeirloomPointByLevelAggregation } from '~/services/character-service'

const props = defineProps<{
  character: Character
  userExperienceMultiplier: number
}>()

defineEmits<{
  retire: []
}>()

const { t } = useI18n()

const canRetire = computed(() => canRetireValidate(props.character.level))

const [shownConfirmDialog, toggleConfirmDialog] = useToggle()

const retireTableData = computed(() => getHeirloomPointByLevelAggregation())
const heirloomPointByLevel = computed(() => getHeirloomPointByLevel(props.character.level))

const experienceMultiplierBonus = computed(() => getExperienceMultiplierBonus(props.userExperienceMultiplier))

const columns: TableColumn<HeirloomPointByLevelAggregation>[] = [
  {
    accessorKey: 'level',
    header: t('character.settings.retire.loomPointsTable.cols.level'),
  },
  {
    accessorKey: 'points',
    header: t('character.settings.retire.loomPointsTable.cols.loomsPoints'),
  },
]
</script>

<template>
  <UTooltip
    :ui="{
      content: 'max-w-96',
    }"
  >
    <UButton
      variant="outline"
      :disabled="!canRetire"
      size="lg"
      block
      icon="crpg:child"
      data-aq-character-action="retire"
      :label="$t('character.settings.retire.title')"
      @click="toggleConfirmDialog(true)"
    />

    <template #content>
      <div class="space-y-3">
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
                    +{{ $n(experienceMultiplierByGeneration, 'percent', { minimumFractionDigits: 0 }) }}
                  </span>
                </template>
                <template #maxMultiplierBonus>
                  <span class="text-content-100">
                    +{{ $n(maxExperienceMultiplierForGeneration - 1, 'percent', { minimumFractionDigits: 0 }) }}
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
                  <UIcon
                    name="crpg:blacksmith" class="
                      inline-block size-6 text-primary
                    "
                  />
                </template>
              </i18n-t>
            </template>
          </i18n-t>
        </div>

        <UTable
          :data="retireTableData"
          :columns
          :ui="{
            th: 'p-2',
            td: 'p-2',
          }"
        />
      </div>
    </template>
  </UTooltip>

  <AppConfirmActionDialog
    v-if="shownConfirmDialog"
    open
    :title="$t('character.settings.respecialize.dialog.title')"
    :name="character.name"
    :confirm-label="$t('action.confirm')"
    @cancel="toggleConfirmDialog(false);"
    @confirm="() => {
      $emit('retire');
      toggleConfirmDialog(false);
    }"
    @update:open="toggleConfirmDialog(false)"
  >
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
          <AppLoom :point="heirloomPointByLevel" />
        </template>
        <template #multiplierBonus>
          <span class="font-bold text-success">
            +{{ $n(experienceMultiplierBonus, 'percent', { minimumFractionDigits: 0 }) }}
          </span>
        </template>
        <template #resetLevel>
          <span class="font-bold text-error">1</span>
        </template>
      </i18n-t>
    </template>
  </AppConfirmActionDialog>

  <!-- <template #popper="{ hide }">
      <ConfirmActionForm
        :name="`${character.name} - ${character.level}`"
        :confirm-label="$t('action.apply')"
        @cancel="hide"
        @confirm="
          () => toggleRetireConfirmTooltip(true)
        "
      >
      <AppConfirmActionPopover
        :shown="shownRetireConfirmTooltip"
        @confirm="
          () => {
            onRetireCharacter();
            hide();
          }
        "
      />
    </template> -->
</template>
