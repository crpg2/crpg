<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

import type { ClanArmoryItem } from '~/models/clan'
import type { UserPublic } from '~/models/user'

import { CLAN_MEMBER_ROLE } from '~/models/clan'
import { isOwnClanArmoryItem } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

const { borrower, clanArmoryItem, lender } = defineProps<{
  clanArmoryItem: ClanArmoryItem
  lender: UserPublic
  borrower: UserPublic | null
}>()

const emit = defineEmits<{
  borrow: []
  remove: []
  return: []
}>()

const { clanMemberRole, user } = toRefs(useUserStore())

const { t } = useI18n()

const isOwnArmoryItem = computed(() => isOwnClanArmoryItem(clanArmoryItem, user.value!.id))
const canReturn = computed(() => borrower?.id === user.value!.id || clanMemberRole.value === CLAN_MEMBER_ROLE.Leader)

const itemActions = computed(() => {
  const result: DropdownMenuItem[] = []

  if (isOwnArmoryItem.value) {
    result.push({
      label: t('clan.armory.item.remove.title'),
      onSelect: () => {
        emit('remove')
      },
    })
  }

  else if (!borrower) {
    result.push({
      slot: 'borrow' as const,
      label: t('clan.armory.item.borrow.title'),
      onSelect: () => {
        emit('borrow')
      },
    })
  }

  else if (canReturn.value) {
    result.push({
      label: t('clan.armory.item.return.title'),
      onSelect: () => {
        emit('return')
      },
    })
  }

  return result
})
</script>

<template>
  <ItemDetail :item="clanArmoryItem.item">
    <template #badges-bottom-right>
      <ClanArmoryItemRelationBadge
        :lender="lender"
        :borrower="borrower"
      />
    </template>

    <template v-if="itemActions.length" #actions>
      <UDropdownMenu :items="itemActions" size="xl">
        <UButton variant="subtle" color="neutral" size="xl" icon="crpg:dots" />

        <template #borrow>
          <i18n-t
            scope="global"
            keypath="clan.armory.item.borrow.title"
          >
            <template #user>
              <UserMedia
                :user="lender"
                hidden-platform
                hidden-clan
              />
            </template>
          </i18n-t>
        </template>
      </UDropdownMenu>
    </template>
  </ItemDetail>
</template>
