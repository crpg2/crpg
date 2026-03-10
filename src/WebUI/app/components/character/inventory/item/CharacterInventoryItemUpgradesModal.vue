<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { AppCoin, AppLoom, LazyCharacterInventoryItemReforgeConfirmDialog, LazyCharacterInventoryItemUpgradeConfirmDialog, UButton, UiTooltipContent, UTable, UTooltip } from '#components'

import type { ReforgeCost } from '~/composables/item/use-item-reforge'
import type { ItemFlat } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useItemReforge } from '~/composables/item/use-item-reforge'
import { useUser } from '~/composables/user/use-user'
import { getItemAggregations } from '~/services/item-search-service'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItemUpgrades, getRankColor, getRelativeEntries } from '~/services/item-service'

const { userItem } = defineProps<{
  userItem: UserItem
}>()

const emit = defineEmits<{
  upgrade: [upgradeRank: number]
  reforge: []
}>()

const { t } = useI18n()
const { user } = useUser()

const item = computed(() => createItemIndex([userItem.item]).at(0)!)
const aggregationConfig = computed(() => getItemAggregations(item.value, false))

const {
  state: itemUpgrades,
  isLoading: isLoadingitemUpgrades,
} = useAsyncState(async () => createItemIndex(await getItemUpgrades(userItem.item.baseId)), [])

const baseItem = computed(() => itemUpgrades.value.find(iu => iu.rank === 0))

const relativeEntries = computed(() => baseItem.value ? getRelativeEntries(baseItem.value, aggregationConfig.value) : {})

const {
  canReforge,
  reforgeCost,
  reforgeCostTable,
  validation: reforgeValidation,
} = useItemReforge(userItem.item.rank)

const overlay = useOverlay()

async function upgrade(nextItem: ItemFlat) {
  const upgradeItemConfirm = overlay.create(LazyCharacterInventoryItemUpgradeConfirmDialog)
  if (!(await upgradeItemConfirm.open({
    item: item.value,
    nextItem,
  }))) {
    return
  }

  emit('upgrade', nextItem.rank)
}

async function reforge() {
  if (!baseItem.value) {
    return
  }

  const reforgeItemConfirm = overlay.create(LazyCharacterInventoryItemReforgeConfirmDialog)
  if (!(await reforgeItemConfirm.open({
    item: item.value,
    newItem: baseItem.value,
    reforgeCost: reforgeCost.value,
  }))) {
    return
  }

  emit('reforge')
}

const renderItemAction = (_item: ItemFlat) => {
  // reforge
  if (_item.rank === 0 && reforgeValidation.value.rank) {
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

    return h(UTooltip, {}, {
      default: () => h(UButton, {
        variant: 'subtle',
        disabled: !canReforge.value,
        onClick: () => reforge(),
      }, () => [
        t('action.reforge'),
        h(AppCoin, { value: reforgeCost.value }),
      ]),
      content: () => h('div', { class: 'space-y-4' }, [
        h(UiTooltipContent, {
          title: t('character.inventory.item.reforge.tooltip.title'),
          description: t('character.inventory.item.reforge.tooltip.description'),
          validation: !reforgeValidation.value.gold
            ? t('character.inventory.item.reforge.validation.gold')
            : undefined,
        }),
        // @ts-expect-error TODO:
        h(UTable, {
          data: reforgeCostTable.value,
          columns: reforgeTableInfoColumns,
        }),
      ]),
    })
  }

  if (_item.rank <= item.value.rank) {
    return null
  }

  // upgrade
  const notEnoughtPoints = user.value!.heirloomPoints < _item.rank - item.value.rank

  return h(UTooltip, {}, {
    default: () => h(UButton, {
      variant: 'subtle',
      disabled: notEnoughtPoints,
      onClick: () => upgrade(_item),
    }, () => [
      t('action.upgrade'),
      h(AppLoom, { point: _item.rank - item.value.rank }),
    ]),
    content: () => h(UiTooltipContent, {
      title: t('character.inventory.item.upgrade.tooltip.title'),
      description: t('character.inventory.item.upgrade.tooltip.description'),
      validation: notEnoughtPoints
        ? t('character.inventory.item.upgrade.validation.loomPoints')
        : undefined,
    }),
  })
}
</script>

<template>
  <UModal
    :ui="{
      content: 'max-w-5/6',
      title: 'flex items-center justify-center gap-4',
      body: 'p-0!',
    }"
  >
    <template #title>
      <UiTextView variant="h2">
        {{ $t('character.inventory.item.upgrade.upgradesTitle') }}
      </UiTextView>

      <AppLoom :point="user!.heirloomPoints" size="xl" />
      <AppCoin :value="user!.gold" size="xl" />
    </template>

    <template #body>
      <ItemTableUpgrades
        :current-rank="userItem.item.rank"
        :loading="isLoadingitemUpgrades"
        :items="itemUpgrades"
        :aggregation-config
        :compare-items-result="relativeEntries"
      >
        <template #name-caption="{ row }">
          <component :is="renderItemAction(row.original)" />
        </template>
      </ItemTableUpgrades>
    </template>
  </UModal>
</template>
