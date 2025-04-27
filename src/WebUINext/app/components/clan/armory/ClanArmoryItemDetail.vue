<script setup lang="ts">
import type { ClanArmoryItem } from '~/models/clan'
import type { UserPublic } from '~/models/user'

import { ClanMemberRole } from '~/models/clan'
import { isClanArmoryItemInInventory, isOwnClanArmoryItem } from '~/services/clan-service'
import { useUserStore } from '~/stores/user'

const { borrower, clanArmoryItem, lender } = defineProps<{
  clanArmoryItem: ClanArmoryItem
  lender: UserPublic
  borrower: UserPublic | null
}>()

defineEmits<{
  borrow: [id: number]
  remove: [id: number]
  return: [id: number]
}>()

const { clanMemberRole, user, userItems } = toRefs(useUserStore())

const isOwnArmoryItem = computed(() => isOwnClanArmoryItem(clanArmoryItem, user.value!.id))
const isInInventory = computed(() => isClanArmoryItemInInventory(clanArmoryItem, userItems.value))
const canReturn = computed(
  () => borrower?.id === user.value!.id || clanMemberRole.value === ClanMemberRole.Leader,
)
</script>

<template>
  <ItemDetail :item="clanArmoryItem.item">
    <template #badges-bottom-right>
      <ClanArmoryItemRelationBadge
        :lender="lender"
        :borrower="borrower"
      />
    </template>

    <template #actions>
      <div>ddd</div>
      <!-- <ConfirmActionTooltip
        v-if="isOwnArmoryItem"
        class="flex-auto"
        :confirm-label="$t('action.ok')"
        :title="$t('clan.armory.item.remove.confirm.description')"
        @confirm="$emit('remove', clanArmoryItem.userItemId)"
      >
        <OButton
          variant="warning"
          expanded
          rounded
          size="lg"
          icon-left="armory"
          :label="$t('clan.armory.item.remove.title')"
        />
      </ConfirmActionTooltip>

      <div
        v-else-if="!borrower"
        class="flex w-full flex-col items-center justify-center gap-2"
      >
        <OButton
          v-tooltip="{
            disabled: !isInInventory,
            content: $t('clan.armory.item.borrow.validation.isInInventory'),
            popperClass: 'v-popper--theme-tooltip-danger',
          }"
          variant="secondary"
          expanded
          rounded
          size="lg"
          :disabled="isInInventory"
          @click="$emit('borrow', clanArmoryItem.userItemId)"
        >
          <i18n-t
            scope="global"
            keypath="clan.armory.item.borrow.title"
            tag="div"
            class="flex items-center gap-2"
          >
            <template #user>
              <UserMedia
                :user="lender"
                hidden-platform
                hidden-clan
              />
            </template>
          </i18n-t>
        </OButton>
      </div>

      <template v-else>
        <OButton
          v-if="canReturn"
          variant="secondary"
          icon-left="armory"
          expanded
          rounded
          size="lg"
          :label="$t('clan.armory.item.return.title')"
          @click="$emit('return', clanArmoryItem.userItemId)"
        />

        <OButton
          v-else
          v-tooltip="{
            content: $t('clan.armory.item.borrow.validation.borrowed'),
            popperClass: 'v-popper--theme-tooltip-danger',
          }"
          variant="secondary"
          disabled
          expanded
          rounded
          size="lg"
        >
          <i18n-t
            scope="global"
            keypath="clan.armory.item.borrowed.title"
            tag="div"
            class="flex items-center gap-2"
          >
            <template #user>
              <UserMedia
                :user="borrower"
                hidden-platform
                hidden-clan
              />
            </template>
          </i18n-t>
        </OButton>
      </template>
    </template> -->
    </template>
  </ItemDetail>
</template>
