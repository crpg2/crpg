<script setup lang="ts">
import { PartyStatus } from '~/models/strategus/party'
import { partyKey, settlementKey } from '~/symbols/strategus'

definePage({
  meta: {
    roles: ['User', 'Moderator', 'Admin'],
  },
})

const settlement = injectStrict(settlementKey)
const {
  party,
  toggleRecruitTroops,
  isTogglingRecruitTroops,
} = injectStrict(partyKey)
</script>

<template>
  <div>
    <div class="prose prose-invert">
      <div>TODO: Settlement town center</div>
      <pre>{{ settlement }}</pre>
      <ul>
        <li>--</li>
        <li>--</li>
      </ul>
    </div>

    <!-- <OButton
      variant="primary"
      size="lg"
      :label="
        party!.status !== PartyStatus.RecruitingInSettlement
          ? 'Start recruiting troops'
          : 'Stop recruiting troops'
      "
      :loading="isTogglingRecruitTroops"
      :disabled="isTogglingRecruitTroops"
      @click="toggleRecruitTroops"
    /> -->
  </div>
</template>
