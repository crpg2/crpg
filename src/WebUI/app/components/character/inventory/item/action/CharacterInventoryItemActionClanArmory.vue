<script setup lang="ts">
import type { UserItem } from '~/models/user'

import { useUser } from '~/composables/user/use-user'

const {
  userItem,
} = defineProps<{
  userItem: UserItem
}>()

defineEmits<{
  add: []
  remove: []
  return: []
}>()

const { user } = useUser()

const isOwnArmoryItem = computed(() => userItem.clanArmoryLender && userItem.userId === user.value!.id)
</script>

<template>
  <UTooltip
    :text="!userItem.clanArmoryLender
      ? $t('clan.armory.item.add.title')
      : isOwnArmoryItem
        ? $t('clan.armory.item.remove.title')
        : $t('clan.armory.item.return.title')"
  >
    <UButton
      variant="subtle"
      color="neutral"
      size="xl"
      block
      icon="crpg:armory"
      @click="!userItem.clanArmoryLender
        ? $emit('add')
        : isOwnArmoryItem
          ? $emit('remove')
          : $emit('return')"
    />
  </UTooltip>
</template>
