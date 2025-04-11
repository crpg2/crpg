<script setup lang="ts">
import { Tooltip } from 'floating-vue'
import { I18nT } from 'vue-i18n'

import type { ActivityLog, ActivityLogMetadataDicts } from '~/models/activity-logs'
import type { ClanMemberRole } from '~/models/clan'
import type { UserPublic } from '~/models/user'

import Coin from '~/components/app/Coin.vue'
import Loom from '~/components/app/Loom.vue'
import CharacterMedia from '~/components/character/CharacterMedia.vue'
import ClanRole from '~/components/clan/ClanRole.vue'
import Tag from '~/components/ui/Tag.vue'
import UserClan from '~/components/user/UserClan.vue'
import UserMedia from '~/components/user/UserMedia.vue'
import { getItemImage } from '~/services/item-service'
import { n } from '~/services/translate-service'

const { keypath, activityLog, dict } = defineProps<{
  keypath: string
  activityLog: ActivityLog
  dict: ActivityLogMetadataDicts
}>()

defineEmits<{
  read: []
  delete: []
}>()

const slots = defineSlots<{
  user: (props: { user: UserPublic }) => any
}>()

const getClanById = (clanId: number) => dict.clans.find(({ id }) => id === clanId)

const getUserById = (userId: number) => dict.users.find(({ id }) => id === userId)

const getCharacterById = (characterId: number) => dict.characters.find(({ id }) => id === characterId)

const renderStrong = (value: string) => h('strong', { class: 'font-bold text-content-100' }, value)

const renderDamage = (value: string) => h('strong', { class: 'font-bold text-status-danger' }, n(Number(value)))

const renderUserClan = (clanId: number) => {
  const clan = getClanById(clanId)
  return clan
    ? h(UserClan, { clan, class: 'inline-flex items-center gap-1 align-middle' })
    : renderStrong(String(clanId))
}

const renderUser = (userId: number) => {
  const user = getUserById(userId)

  return user
    ? slots?.user({ user }) || h(UserMedia, { user, class: 'text-content-100' })
    : renderStrong(String(userId))
}

const renderCharacter = (characterId: number) => {
  const character = getCharacterById(Number(characterId))
  return character
    ? h(CharacterMedia, {
      character, // TODO: FIXME:
      class: 'inline-flex items-center gap-1 align-middle font-bold text-content-100',
    })
    : renderStrong(String(characterId))
}

const renderItem = (itemId: string) => {
  return h(
    'span',
    { class: 'inline' },
    h(
      Tooltip,
      {
        placement: 'auto',
        class: 'inline-block',
      },
      {
        default: () => renderStrong(itemId),
        popper: () =>
          h('img', {
            src: getItemImage(itemId),
            class: 'h-full w-full object-contain',
          }),
      },
    ),
  )
}

const renderGold = (value: number) => h(Coin, { value })

const renderLoom = (point: number) => h(Loom, { point })

const Render = () => {
  const {
    metadata: {
      clanId,
      oldClanMemberRole,
      newClanMemberRole,
      userId,
      targetUserId,
      actorUserId,
      characterId,
      generation,
      level,
      gold,
      price,
      refundedGold,
      heirloomPoints,
      refundedHeirloomPoints,
      itemId,
      userItemId,
      experience,
      damage,
      instance,
      gameMode,
      oldName,
      newName,
      message,
    },
  } = activityLog

  return h(
    I18nT,
    {
      tag: 'div',
      scope: 'global',
      keypath,
      class: 'leading-relaxed',
    },
    {
      clan: () => renderUserClan(Number(clanId)),
      oldClanMemberRole: () => h(ClanRole, { role: oldClanMemberRole as ClanMemberRole }),
      newClanMemberRole: () => h(ClanRole, { role: newClanMemberRole as ClanMemberRole }),
      ...((activityLog.userId || userId) && { user: () => renderUser(Number(activityLog.userId || userId)) }),
      ...(targetUserId && { targetUser: () => renderUser(Number(targetUserId)) }),
      ...(actorUserId && { actorUser: () => renderUser(Number(actorUserId)) }),
      ...(characterId && { character: () => renderCharacter(Number(characterId)) }),
      ...(generation && { generation: () => renderStrong(generation) }),
      ...(level && { level: () => renderStrong(level) }),
      ...(gold && { gold: () => renderGold(Number(gold)) }),
      ...(price && { price: () => renderGold(Number(price)) }),
      ...(refundedGold && { refundedGold: () => renderGold(Number(refundedGold)) }),
      ...(heirloomPoints && { heirloomPoints: () => renderLoom(Number(heirloomPoints)) }),
      ...(refundedHeirloomPoints && { refundedHeirloomPoints: () => renderLoom(Number(refundedHeirloomPoints)) }),
      ...(itemId && { item: () => renderItem(itemId) }),
      ...(userItemId && { userItem: () => renderItem(userItemId) }),
      ...(experience && { experience: () => renderStrong(n(Number(experience))) }),
      ...(damage && { damage: () => renderDamage(damage) }),
      ...(instance && { instance: () => h(Tag, { variant: 'info', label: instance }) }),
      ...(gameMode && { gameMode: () => h(Tag, { variant: 'info', label: gameMode }) }),
      ...(oldName && { oldName: () => renderStrong(oldName) }),
      ...(newName && { newName: () => renderStrong(newName) }),
      ...(message && { message: () => renderStrong(message) }),
    },
  )
}
</script>

<template>
  <Render />
</template>
