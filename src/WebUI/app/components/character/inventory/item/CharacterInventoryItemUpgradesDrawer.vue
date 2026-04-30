<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import type { ReforgeCost } from '~/composables/item/use-item-reforge'
import type { ItemFlat } from '~/models/item'
import type { UserItem } from '~/models/user'

import { AppCoin, AppLoom, LazyCharacterInventoryItemReforgeConfirmDialog, LazyCharacterInventoryItemUpgradeConfirmDialog, UButton, UiTooltipContent, UTable, UTooltip } from '#components'
import { useItemReforge } from '~/composables/item/use-item-reforge'
import { useUser } from '~/composables/user/use-user'
import { getItemAggregations } from '~/services/item-search-service'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { checkAvailabilityUpgradeOrReforgeUserItem, getItemUpgrades, getRankColor, getRelativeEntries } from '~/services/item-service'

const { userItem } = defineProps<{
  userItem: UserItem
}>()

const emit = defineEmits<{
  upgrade: [upgradeRank: number]
  reforge: []
  close: [boolean]
}>()

const { t } = useI18n()
const { user } = useUser()

const availableUpgradeOrReforgeUserItem = computed(() => checkAvailabilityUpgradeOrReforgeUserItem(t, userItem))

const {
  state: itemUpgrades,
  isLoading: isLoadingitemUpgrades,
} = useAsyncState(async () => createItemIndex(await getItemUpgrades(userItem.item.baseId)), [])

const aggregationConfig = computed(() => itemUpgrades.value.length ? getItemAggregations(itemUpgrades.value.at(0)!, false) : {})

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
    item: {
      id: userItem.item.id,
      baseId: userItem.item.baseId,
      name: userItem.item.name,
      rank: userItem.item.rank,
    },
    nextItem: {
      id: nextItem.id,
      baseId: nextItem.baseId,
      name: nextItem.name,
      rank: nextItem.rank,
    },
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
    item: {
      id: userItem.item.id,
      baseId: userItem.item.baseId,
      name: userItem.item.name,
      rank: userItem.item.rank,
    },
    newItem: {
      name: baseItem.value.name,
      baseId: baseItem.value.baseId,
      id: baseItem.value.id,
      rank: baseItem.value.rank,
    },
    reforgeCost: reforgeCost.value,
  }))) {
    return
  }

  emit('reforge')
}

const renderItemAction = (_item: ItemFlat) => {
  // reforge
  if (_item.rank === 0 && reforgeValidation.value.rank) {
    // TODO: to cmp
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
        disabled: !availableUpgradeOrReforgeUserItem.value[0] || !canReforge.value,
        onClick: () => reforge(),
      }, () => [
        t('action.reforge'),
        h(AppCoin, { value: reforgeCost.value }),
      ]),
      content: () => h('div', { class: 'space-y-4' }, [
        h(UiTooltipContent, {
          title: t('character.inventory.item.reforge.tooltip.title'),
          description: t('character.inventory.item.reforge.tooltip.description'),
          validation: !availableUpgradeOrReforgeUserItem.value[0]
            ? availableUpgradeOrReforgeUserItem.value[1]
            : !reforgeValidation.value.gold
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

  if (_item.rank <= userItem.item.rank) {
    return null
  }

  // upgrade
  const notEnoughtPoints = user.value!.heirloomPoints < _item.rank - userItem.item.rank

  return h(UTooltip, {}, {
    default: () => h(UButton, {
      variant: 'subtle',
      disabled: !availableUpgradeOrReforgeUserItem.value[0] || notEnoughtPoints,
      onClick: () => upgrade(_item),
    }, () => [
      t('action.upgrade'),
      h(AppLoom, { point: _item.rank - userItem.item.rank }),
    ]),
    content: () => h(UiTooltipContent, {
      title: t('character.inventory.item.upgrade.tooltip.title'),
      description: t('character.inventory.item.upgrade.tooltip.description'),
      validation: !availableUpgradeOrReforgeUserItem.value[0]
        ? availableUpgradeOrReforgeUserItem.value[1]
        : notEnoughtPoints
          ? t('character.inventory.item.upgrade.validation.loomPoints')
          : undefined,
    }),
  })
}

const onCancel = () => {
  emit('close', false)
}
</script>

<template>
  <UDrawer
    direction="top"
    :handle="false"
    :ui="{
      container: 'w-full max-w-(--ui-container) mx-auto',
      header: 'flex items-center justify-center gap-4',
    }"
    @close="onCancel"
  >
    <template #header>
      <div class="flex flex-1 items-center justify-center gap-4">
        <UiTextView variant="h2">
          {{ $t('character.inventory.item.upgrade.upgradesTitle') }}
        </UiTextView>
        <AppLoom :point="user!.heirloomPoints" size="xl" />
        <AppCoin :value="user!.gold" size="xl" />
      </div>

      <div class="mr-0 ml-auto">
        <UButton color="neutral" variant="ghost" icon="i-lucide-x" @click="onCancel" />
      </div>
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
  </UDrawer>
</template>
