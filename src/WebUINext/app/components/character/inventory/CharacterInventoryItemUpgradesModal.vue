<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { AppCoin, AppLoom, LazyCharacterInventoryActionItemReforgeConfirmDialog, LazyCharacterInventoryActionItemUpgradeConfirmDialog } from '#components'

import type { ReforgeCost } from '~/composables/item/use-item-reforge'
import type { UserItem } from '~/models/user'

import { useItemReforge } from '~/composables/item/use-item-reforge'
import { useItemUpgrades } from '~/composables/item/use-item-upgrades'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItemAggregations, getRankColor } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

const { userItem } = defineProps<{
  userItem: UserItem
}>()

const emit = defineEmits<{
  upgrade: []
  reforge: []
}>()

const userStore = useUserStore()
const { t } = useI18n()

// TODO:
const item = computed(() => createItemIndex([userItem.item]).at(0)!)
const aggregationConfig = computed(() => getItemAggregations(item.value, false))

const {
  baseItem,
  canUpgrade,
  isLoading,
  itemUpgrades,
  nextItem,
  relativeEntries,
  validation: upgradeValidation,
} = useItemUpgrades({
  item: { baseId: userItem.item.baseId, id: userItem.item.id, rank: userItem.item.rank },
  aggregationConfig: aggregationConfig.value,
})

const {
  canReforge,
  reforgeCost,
  reforgeCostTable,
  validation: reforgeValidation,
} = useItemReforge(userItem.item.rank)

const reforgeTableInfoColumns: TableColumn<ReforgeCost>[] = [
  {
    header: t('character.inventory.item.reforge.tooltip.costTable.cols.rank.label'),
    cell: ({ row }) => h('span', { style: {
      color: getRankColor(row.original.points),
    } }, `+${row.index + 1}`),
  },
  {
    accessorKey: 'cost',
    header: t('character.inventory.item.reforge.tooltip.costTable.cols.cost.label'),
    cell: ({ row }) => h(AppCoin, { value: row.original.cost }),
  },
  {
    accessorKey: 'points',
    header: t('character.inventory.item.reforge.tooltip.costTable.cols.looms.label'),
    cell: ({ row }) => h(AppLoom, { point: row.original.points }),
  },
]

const overlay = useOverlay()

const upgradeItemConfirm = overlay.create(LazyCharacterInventoryActionItemUpgradeConfirmDialog)

async function upgrade() {
  if (!(await upgradeItemConfirm.open({
    item: item.value,
    nextItem: nextItem.value,
  }))) {
    return
  }

  emit('upgrade')
}

const reforgeItemConfirm = overlay.create(LazyCharacterInventoryActionItemReforgeConfirmDialog)

async function reforge() {
  if (!baseItem.value) {
    return
  }

  if (!(await reforgeItemConfirm.open({
    item: item.value,
    newItem: baseItem.value,
    reforgeCost: reforgeCost.value,
  }))) {
    return
  }

  emit('reforge')
}
</script>

<template>
  <UModal
    :ui="{
      content: 'max-w-5/6',
      title: 'flex items-center justify-center gap-4',
    }"
  >
    <template #title>
      <UiTextView variant="h3">
        {{ $t('character.inventory.item.upgrade.upgradesTitle') }}
      </UiTextView>

      <AppLoom :point="userStore.user!.heirloomPoints" />
      <AppCoin :value="userStore.user!.gold" />

      <UTooltip>
        <UButton
          variant="subtle"
          size="lg"
          :disabled="!canUpgrade"
          @click="upgrade"
        >
          {{ $t('action.upgrade') }}
          <AppLoom :point="1" />
        </UButton>

        <template #content>
          <UiTooltipContent
            :title="$t('character.inventory.item.upgrade.tooltip.title')"
            :description="$t('character.inventory.item.upgrade.tooltip.description')"
            :validation="!upgradeValidation.maxRank
              ? $t('character.inventory.item.upgrade.validation.maxRank')
              : !upgradeValidation.points
                ? $t('character.inventory.item.upgrade.validation.loomPoints')
                : undefined
            "
          />
        </template>
      </UTooltip>

      <UTooltip>
        <UButton
          variant="subtle"
          size="lg"
          :disabled="!canReforge"
          @click="reforge"
        >
          {{ $t('action.reforge') }}
          <AppCoin
            v-if="reforgeValidation.rank"
            :value="reforgeCost"
          />
        </UButton>

        <template #content>
          <div class="space-y-4">
            <UiTooltipContent
              :title="$t('character.inventory.item.reforge.tooltip.title')"
              :description="$t('character.inventory.item.reforge.tooltip.description')"
              :validation="!reforgeValidation.rank
                ? $t('character.inventory.item.reforge.validation.rank')
                : !reforgeValidation.gold
                  ? $t('character.inventory.item.reforge.validation.gold')
                  : undefined
              "
            />
            <UTable
              class="rounded-md border border-muted"
              :data="reforgeCostTable"
              :columns="reforgeTableInfoColumns"
            />
          </div>
        </template>
      </UTooltip>
    </template>

    <template #body>
      <ItemTableUpgrades
        with-header
        :loading="isLoading"
        :items="itemUpgrades"
        :aggregation-config
        :compare-items-result="relativeEntries"
      />
    </template>
  </UModal>
</template>
