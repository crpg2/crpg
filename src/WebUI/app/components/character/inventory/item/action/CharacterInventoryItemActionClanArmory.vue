<script setup lang="ts">
import type { UserItem } from '~/models/user'

import { useUser } from '~/composables/user/use-user'
import { checkAvailabilityManageClanArmoryUserItem } from '~/services/item-service'

const { userItem } = defineProps<{
  userItem: UserItem
}>()

defineEmits<{
  add: []
  remove: []
  return: []
}>()

const { t } = useI18n()

const { user } = useUser()

const isOwnArmoryItem = computed(() => userItem.clanArmoryLender && userItem.userId === user.value!.id)
const availableManageClanArmoryUserItem = computed(() => checkAvailabilityManageClanArmoryUserItem(t, userItem))
</script>

<template>
  <UTooltip :content="{ side: 'right' }">
    <UButton
      variant="subtle"
      color="neutral"
      block
      :icon="!userItem.clanArmoryLender ? 'crpg:armory-add' : 'crpg:armory-return'"
      :disabled="!availableManageClanArmoryUserItem[0]"
      @click="!userItem.clanArmoryLender
        ? $emit('add')
        : isOwnArmoryItem
          ? $emit('remove')
          : $emit('return')"
    />

    <template #content>
      <UiTooltipContent
        :title="!userItem.clanArmoryLender
          ? $t('clan.armory.item.add.title')
          : isOwnArmoryItem
            ? $t('clan.armory.item.remove.title')
            : $t('clan.armory.item.return.title')"
        :validation="availableManageClanArmoryUserItem[1]"
      />
    </template>
  </UTooltip>
</template>
