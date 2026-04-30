<script setup lang="ts">
import type { ClanArmoryItem } from '~/models/clan'
import type { CompareItemsResult } from '~/models/item'

import { useUser } from '~/composables/user/use-user'
import { CLAN_MEMBER_ROLE } from '~/models/clan'
import { isOwnClanArmoryItem } from '~/services/clan-service'

const {
  clanArmoryItem,
  compareResult,
} = defineProps<{
  clanArmoryItem: ClanArmoryItem
  compareResult?: CompareItemsResult
}>()

defineEmits<{
  borrow: []
  remove: []
  return: []
}>()

const { clanMemberRole, user } = useUser()

const { t } = useI18n()

const isOwnArmoryItem = computed(() => isOwnClanArmoryItem(clanArmoryItem, user.value!.id))
const canReturn = computed(() => clanArmoryItem.borrower?.id === user.value!.id
  || clanMemberRole.value === CLAN_MEMBER_ROLE.Leader || clanMemberRole.value === CLAN_MEMBER_ROLE.Officer)
</script>

<template>
  <ItemDetail
    :item="clanArmoryItem.item"
    :compare-result="compareResult"
  >
    <template #badges-bottom-right>
      <ClanArmoryItemRelationBadge
        :lender="clanArmoryItem.lender"
        :borrower="clanArmoryItem.borrower"
      />
    </template>

    <template #actions>
      <UTooltip v-if="isOwnArmoryItem" :text="t('clan.armory.item.remove.title')">
        <UButton
          variant="subtle"
          color="neutral"
          size="xl"
          icon="crpg:armory-return"
          @click="$emit('remove')"
        />
      </UTooltip>

      <UTooltip v-else-if="!clanArmoryItem.borrower">
        <UButton
          variant="subtle"
          color="neutral"
          size="xl"
          icon="crpg:armory-take"
          @click="$emit('borrow')"
        />
        <template #content>
          <i18n-t
            scope="global"
            class="flex items-center gap-2"
            tag="div"
            keypath="clan.armory.item.borrow.title"
          >
            <template #user>
              <UserMedia :user="clanArmoryItem.lender" hidden-clan />
            </template>
          </i18n-t>
        </template>
      </UTooltip>

      <UTooltip v-else-if="canReturn" :text="t('clan.armory.item.return.title')">
        <UButton
          variant="subtle"
          color="neutral"
          size="xl"
          icon="crpg:armory-return"
          @click="$emit('return')"
        />
      </UTooltip>
    </template>
  </ItemDetail>
</template>
