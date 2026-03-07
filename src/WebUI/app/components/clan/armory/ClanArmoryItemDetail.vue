<script setup lang="ts">
import type { ClanArmoryItem } from '~/models/clan'
import type { CompareItemsResult } from '~/models/item'
import type { UserPublic } from '~/models/user'

import { useUser } from '~/composables/user/use-user'
import { CLAN_MEMBER_ROLE } from '~/models/clan'
import { isOwnClanArmoryItem } from '~/services/clan-service'

const { borrower, clanArmoryItem, lender, compareResult } = defineProps<{
  clanArmoryItem: ClanArmoryItem
  lender: UserPublic
  compareResult?: CompareItemsResult
  borrower: UserPublic | null
}>()

defineEmits<{
  borrow: []
  remove: []
  return: []
}>()

const { clanMemberRole, user } = useUser()

const { t } = useI18n()

const isOwnArmoryItem = computed(() => isOwnClanArmoryItem(clanArmoryItem, user.value!.id))
const canReturn = computed(() => borrower?.id === user.value!.id || clanMemberRole.value === CLAN_MEMBER_ROLE.Leader)
</script>

<template>
  <ItemDetail
    :item="clanArmoryItem.item"
    :compare-result="compareResult"
  >
    <template #badges-bottom-right>
      <ClanArmoryItemRelationBadge
        :lender="lender"
        :borrower="borrower"
      />
    </template>

    <template #actions>
      <UTooltip v-if="isOwnArmoryItem" :text="t('clan.armory.item.remove.title')">
        <UButton
          variant="subtle"
          color="neutral"
          size="xl"
          icon="i-lucide-undo-2"
          @click="$emit('remove')"
        />
      </UTooltip>

      <UTooltip v-else-if="!borrower">
        <UButton
          variant="subtle"
          color="neutral"
          size="xl"
          icon="i-lucide-hand"
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
              <UserMedia :user="lender" />
            </template>
          </i18n-t>
        </template>
      </UTooltip>

      <UTooltip v-else-if="canReturn" :text="t('clan.armory.item.return.title')">
        <UButton
          variant="subtle"
          color="neutral"
          size="xl"
          icon="i-lucide-undo-2"
          @click="$emit('return')"
        />
      </UTooltip>
    </template>
  </ItemDetail>
</template>
