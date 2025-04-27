<script setup lang="ts">
import type { TableColumn } from '@nuxt/ui'

import { AppCoin, AppLoom } from '#components'

import type { ReforgeCost } from '~/composables/item/use-item-reforge'
import type { ItemRank } from '~/models/item'
import type { UserItem } from '~/models/user'

import { useItemReforge } from '~/composables/item/use-item-reforge'
import { useItemUpgrades } from '~/composables/item/use-item-upgrades'
import { createItemIndex } from '~/services/item-search-service/indexator'
import { getItemAggregations, getRankColor } from '~/services/item-service'
import { useUserStore } from '~/stores/user'

const { userItem } = defineProps<{
  open: boolean
  userItem: UserItem
}>()

const emit = defineEmits<{
  upgrade: []
  reforge: []
}>()

const userStore = useUserStore()
const { t } = useI18n()

// TODO:
const item = computed(() => createItemIndex([userItem.item])[0]!)
const aggregationConfig = computed(() => getItemAggregations(item.value!, false))

const {
  baseItem,
  canUpgrade,
  isLoading,
  itemUpgrades,
  nextItem,
  relativeEntries,
  validation: upgradeValidation,
} = useItemUpgrades({
  item: {
    baseId: userItem.item.baseId,
    id: userItem.item.id,
    rank: userItem.item.rank,
  },
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
      color: getRankColor(row.original.points as ItemRank),
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

const [shownConfirmUpgradeDialog, toggleConfirmUpgradeDialog] = useToggle()
const [shownConfirmReforgeDialog, toggleConfirmReforgeDialog] = useToggle()
</script>

<template>
  <UModal
    :open
    :ui="{
      content: 'max-w-5/6',
      body: 'pt-4',
      header: 'pb-4',
      title: 'flex items-center justify-between gap-4',
    }"
    :close="{
      size: 'sm',
      color: 'secondary',
      variant: 'solid',
    }"
  >
    <template #title>
      <div class="flex items-center gap-4">
        <h3 class="text-xl font-semibold">
          {{ $t('character.inventory.item.upgrade.upgradesTitle') }}
        </h3>
        <AppLoom :point="userStore.user!.heirloomPoints" />
        <AppCoin :value="userStore.user!.gold" />
      </div>

      <div class="flex items-center gap-4">
        <UTooltip>
          <UButton
            variant="subtle"
            size="lg"
            :disabled="!canUpgrade"
            @click="() => { toggleConfirmUpgradeDialog(true) }"
          >
            {{ $t('action.upgrade') }}
            <AppLoom :point="1" />
          </UButton>

          <template #content>
            <div class="prose prose-invert">
              <h5>
                {{ $t('character.inventory.item.upgrade.tooltip.title') }}
              </h5>
              <p>
                {{ $t('character.inventory.item.upgrade.tooltip.description') }}
              </p>
              <i18n-t
                v-if="!upgradeValidation.maxRank"
                scope="global"
                keypath="character.inventory.item.upgrade.validation.maxRank"
                class="text-status-danger"
                tag="p"
              />
              <i18n-t
                v-else-if="!upgradeValidation.points"
                scope="global"
                keypath="character.inventory.item.upgrade.validation.loomPoints"
                class="text-status-danger"
                tag="p"
              />
            </div>
          </template>
        </UTooltip>

        <UTooltip>
          <UButton
            variant="subtle"
            size="lg"
            :disabled="!canReforge"
            @click="() => { toggleConfirmReforgeDialog(true) }"
          >
            {{ $t('action.reforge') }}
            <AppCoin
              v-if="reforgeValidation.rank"
              :value="reforgeCost"
            />
          </UButton>

          <template #content>
            <div class="space-y-4">
              <div class="prose prose-invert">
                <h5>{{ $t('character.inventory.item.reforge.tooltip.title') }}</h5>
                <p>{{ $t('character.inventory.item.reforge.tooltip.description') }}</p>
              </div>

              <UTable
                class="rounded-md border border-muted"
                :data="reforgeCostTable"
                :columns="reforgeTableInfoColumns"
              />

              <div class="prose prose-invert">
                <i18n-t
                  v-if="!reforgeValidation.rank"
                  scope="global"
                  keypath="character.inventory.item.reforge.validation.rank"
                  class="text-status-danger"
                  tag="p"
                >
                  <template #minimumRank>
                    <span class="font-bold">0</span>
                  </template>
                </i18n-t>
                <i18n-t
                  v-else-if="!reforgeValidation.gold"
                  scope="global"
                  keypath="character.inventory.item.reforge.validation.gold"
                  class="text-status-danger"
                  tag="p"
                />
              </div>
            </div>
          </template>
        </UTooltip>
      </div>
    </template>

    <template #body>
      <div class="relative space-y-6">
        <UiLoading :active="isLoading" />

        <ItemTableUpgrades
          v-if="!isLoading && itemUpgrades.length"
          with-header
          :items="itemUpgrades"
          :aggregation-config
          :compare-items-result="relativeEntries"
        />

        <!-- UPGRADE CONFIRM -->
        <AppConfirmActionDialog
          :open="shownConfirmUpgradeDialog"
          :title="$t('action.confirmation')"
          name="Upgrade item"
          :confirm-label="$t('action.confirm')"
          @confirm="() => {
            emit('upgrade');
            toggleConfirmUpgradeDialog(false)
          }"
          @cancel="toggleConfirmUpgradeDialog(false);"
          @update:open="toggleConfirmUpgradeDialog(false)"
        >
          <template #description>
            <i18n-t
              scope="global"
              keypath="character.inventory.item.upgrade.confirm.description"
              tag="div"
            >
              <template #loomPoints>
                <AppLoom :point="1" />
              </template>
              <template #oldItem>
                <span
                  class="font-bold"
                  :style="{ color: getRankColor(item.rank) }"
                >
                  {{ item.name }}
                </span>
              </template>
              <template #newItem>
                <span
                  class="font-bold"
                  :style="{ color: getRankColor(nextItem!.rank) }"
                >
                  {{ nextItem!.name }}
                </span>
              </template>
            </i18n-t>
          </template>
        </AppConfirmActionDialog>

        <!-- REFORGE CONFIRM -->
        <AppConfirmActionDialog
          :open="shownConfirmReforgeDialog"
          :title="$t('action.confirmation')"
          name="Reforge item"
          :confirm-label="$t('action.confirm')"
          @confirm="() => {
            emit('reforge');
            toggleConfirmReforgeDialog(false)
          }"
          @cancel="toggleConfirmReforgeDialog(false);"
          @update:open="toggleConfirmReforgeDialog(false)"
        >
          <template #description>
            <i18n-t
              scope="global"
              keypath="character.inventory.item.reforge.confirm.description"
              tag="div"
            >
              <template #gold>
                <AppCoin :value="reforgeCost" />
              </template>
              <template #loomPoints>
                <AppLoom :point="item.rank" />
              </template>
              <template #oldItem>
                <span
                  class="font-bold"
                  :style="{ color: getRankColor(item.rank) }"
                >
                  {{ item.name }}
                </span>
              </template>
              <template #newItem>
                <span
                  class="font-bold"
                  :style="{ color: getRankColor(baseItem.rank) }"
                >
                  {{ baseItem.name }}
                </span>
              </template>
            </i18n-t>
          </template>
        </AppConfirmActionDialog>
      </div>
    </template>
  </UModal>
</template>
