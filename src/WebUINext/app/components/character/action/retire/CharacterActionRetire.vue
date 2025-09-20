<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { CharacterActionRetireConfirmDialog } from '#components'
import {
  experienceMultiplierByGeneration,
  maxExperienceMultiplierForGeneration,
  minimumLevel,
  minimumRetirementLevel,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { HeirloomPointByLevelAggregation } from '~/services/character-service'

import { canRetireValidate, getHeirloomPointByLevel, getHeirloomPointByLevelAggregation } from '~/services/character-service'

const { character, userExperienceMultiplier } = defineProps<{
  character: Character
  userExperienceMultiplier: number
}>()

const emit = defineEmits<{
  retire: []
}>()

const { t } = useI18n()

const canRetire = computed(() => canRetireValidate(character.level))

const retireTableData = computed(() => getHeirloomPointByLevelAggregation())

const heirloomPointByLevel = computed(() => getHeirloomPointByLevel(character.level))

const columns: TableColumn<HeirloomPointByLevelAggregation>[] = [
  {
    accessorKey: 'level',
    header: t('character.settings.retire.loomPointsTable.cols.level'),
  },
  {
    accessorKey: 'points',
    header: t('character.settings.retire.loomPointsTable.cols.loomsPoints'),
    cell: (cellProps) => {
      const points = cellProps.getValue()
      return `${points}${points === heirloomPointByLevel.value ? ' (you here)' : ''}`
    },
  },
]

const overlay = useOverlay()

const confirmDialog = overlay.create(CharacterActionRetireConfirmDialog)

async function retire() {
  if (!(await confirmDialog.open({ character, userExperienceMultiplier }))) {
    return
  }

  emit('retire')
}
</script>

<template>
  <UTooltip
    :content="{ side: 'right' }"
    :ui="{
      content: 'max-w-96 max-h-[480px] overflow-y-auto block',
    }"
  >
    <UButton
      variant="outline"
      :disabled="!canRetire"
      size="xl"
      icon="crpg:child"
      data-aq-character-action="retire"
      :label="$t('character.settings.retire.title')"
      @click="retire"
    />

    <template #content>
      <div class="space-y-3">
        <div class="prose">
          <i18n-t
            v-if="!canRetire"
            scope="global"
            keypath="character.settings.retire.tooltip.required"
            class="text-warning"
            tag="h4"
          >
            <template #requiredLevel>
              <span>{{ minimumRetirementLevel }}+</span>
            </template>
          </i18n-t>

          <h3>{{ $t('character.settings.retire.tooltip.title') }}</h3>

          <i18n-t
            scope="global"
            keypath="character.settings.retire.tooltip.desc[0]"
            tag="p"
          >
            <template #resetLevel>
              <span class="font-bold text-highlighted">{{ minimumLevel }}</span>
            </template>
            <template #multiplierBonus>
              <span class="font-bold text-success">
                +{{ $n(experienceMultiplierByGeneration, 'percent', { minimumFractionDigits: 0 }) }}
              </span>
            </template>
            <template #maxMultiplierBonus>
              <span class="font-bold text-success">
                +{{ $n(maxExperienceMultiplierForGeneration - 1, 'percent', { minimumFractionDigits: 0 }) }}
              </span>
            </template>
          </i18n-t>

          <i18n-t
            scope="global"
            keypath="character.settings.retire.tooltip.desc[1]"
            tag="p"
          >
            <template #heirloom>
              <UIcon name="crpg:blacksmith" class="inline-block size-6 text-primary" />
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
</template>
