<script setup lang="ts">
import { getSettlementGarrisonItems } from '~/services/strategus-service/settlement'
import { settlementKey } from '~/symbols/strategus'

definePage({
  meta: {
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const route = useRoute<'StrategusSettlementIdGarrison'>()

const settlement = injectStrict(settlementKey)

//
const { state: garrisonItems, execute: loadGarrisonItems } = useAsyncState(
  () => getSettlementGarrisonItems(Number(route.params.id)),
  [],
  {
    immediate: false,
    resetOnExecute: false,
  },
)

await loadGarrisonItems()
</script>

<template>
  <div>
    <div class="prose prose-invert">
      <div>TODO: Settlement garrison</div>
      <ul>
        <li>Add/Remove troops - owner</li>
        <li>Add troops - clan member</li>
        <li>Troops limit?</li>

        <!--  -->

        <li>Add/Remove items</li>
        <li>Add items - clan member</li>

        <!--  -->

        <li>Settlement gold?</li>
      </ul>
    </div>

    <br>
    <br>
    <br>

    <!--  -->
    <div>
      <div class="grid grid-cols-5 gap-2">
        <div
          v-for="garrisonItem in garrisonItems"
          :key="garrisonItem.item.id"
        >
          <ItemCard

            :item="garrisonItem.item"
          />
          Count:{{ garrisonItem.count }}
        </div>
      </div>
    </div>
  </div>
</template>
