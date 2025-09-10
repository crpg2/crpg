<script setup lang="ts">
import type { Battle, BattleMercenary } from '~/models/strategus/battle'

import { usePagination } from '~/composables/use-pagination'
import { notify } from '~/services/notification-service'
import { removeBattleMercenary } from '~/services/strategus-service/battle'
import { t } from '~/services/translate-service'

const props = defineProps<{
  battle: Battle
  battleMercenaries: BattleMercenary[]
}>()

const emit = defineEmits<{
  mercenaryRemoved: []
}>()

const { pageModel, perPage } = usePagination()

const searchModel = ref<string>('')
const filteredBattleMercenaries = computed(() =>
  props.battleMercenaries.filter(mercenary =>
    mercenary.user.name.toLowerCase().includes(searchModel.value.toLowerCase()),
  ),
)

const RemoveBattleMercenary = async (battleId: number, mercenaryId: number) => {
  await removeBattleMercenary(battleId, mercenaryId)
  notify(t('strategus.battle.mercenary.remove.notify.success'))
  emit('mercenaryRemoved')
}
</script>

<template>
  <div class="mx-auto max-w-4xl">
    <h1 class="pb-4 text-center text-lg">
      {{ $t('strategus.battle.mercenary.title') }}
    </h1>
    <OTable
      v-model:current-page="pageModel"
      :data="filteredBattleMercenaries"
      :per-page="perPage"
      bordered
      :paginated="battleMercenaries.length > perPage"
    >
      <OTableColumn field="user.name">
        <template #header>
          <div class="w-44">
            <OInput
              v-model="searchModel"
              type="text"
              expanded
              clearable
              :placeholder="$t('clan.table.column.name')"
              icon="search"
              rounded
              size="xs"
            />
          </div>
        </template>
        <template #default="{ row: mercenary }: { row: BattleMercenary }">
          <UserMedia
            :user="mercenary.user"
            hidden-clan
          />
        </template>
      </OTableColumn>

      <OTableColumn
        v-slot="{ row: mercenary }: { row: BattleMercenary }"
        field="action"
        position="right"
        :label="$t('strategus.battle.application.table.column.actions')"
        width="160"
      >
        <div class="items-center justify-center">
          <ConfirmActionTooltip
            :confirm-label="$t('action.ok')"
            :title="$t('strategus.battle.mercenary.remove.confirm')"
            placement="bottom"
            @confirm="RemoveBattleMercenary(props.battle.id, mercenary.id)"
          >
            <OButton
              variant="primary"
              inverted
              :label="$t('action.remove')"
              size="xs"
            />
          </ConfirmActionTooltip>
        </div>
      </OTableColumn>

      <template #empty>
        <ResultNotFound />
      </template>
    </OTable>
  </div>
</template>
