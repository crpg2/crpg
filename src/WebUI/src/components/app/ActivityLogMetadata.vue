<script setup lang="ts">
import { Tooltip } from 'floating-vue'
import { I18nT } from 'vue-i18n'

import type { ActivityLog, ActivityLogMetadataDicts } from '~/models/activity-logs'
import type { ClanMemberRole } from '~/models/clan'

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

const getClanById = (clanId: number) => dict.clans.find(({ id }) => id === clanId)

const getUserById = (userId: number) => dict.users.find(({ id }) => id === userId)

const getCharacterById = (characterId: number) =>
  dict.characters.find(({ id }) => id === characterId)

const renderStrong = (value: string) => h('strong', { class: 'font-bold text-content-100' }, value)

const renderDamage = (value: string) => {
  return h('strong', { class: 'font-bold text-status-danger' }, n(Number(value)))
}

const renderUserClan = (clanId: number) => {
  const clan = getClanById(clanId)
  return clan
    ? h(UserClan, { clan, class: 'inline-flex items-center gap-1 align-middle' })
    : renderStrong(String(clanId))
}

const renderUser = (userId: number) => {
  const user = getUserById(userId)
  return user
    ? h(UserMedia, {
      user,
      class: 'inline-flex items-center gap-1 align-middle font-bold text-content-100',
    })
    : renderStrong(String(userId))
}

const renderCharacter = () => {
  const character = getCharacterById(Number(activityLog.metadata.characterId))
  return character
    ? h(CharacterMedia, {
      character,
      class: 'inline-flex items-center gap-1 align-middle font-bold text-content-100',
    })
    : renderStrong(activityLog.metadata.characterId)
}

const renderItem = (itemId: string) => {
  return h(
    'span',
    {
      class: 'inline',
    },
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
  const { metadata } = activityLog

  return h(
    I18nT,
    {
      tag: 'div',
      scope: 'global',
      keypath,
      class: 'leading-relaxed',
    },
    {
      clan: () => renderUserClan(Number(metadata.clanId)),
      oldClanMemberRole: () => h(ClanRole, { role: metadata.oldClanMemberRole as ClanMemberRole }),
      newClanMemberRole: () => h(ClanRole, { role: metadata.newClanMemberRole as ClanMemberRole }),
      ...((activityLog.userId || metadata.userId) && {
        user: () => renderUser(Number(activityLog.userId || metadata.userId)),
      }),
      ...(metadata.targetUserId && {
        targetUser: () => renderUser(Number(metadata.targetUserId)),
      }),
      ...(metadata.actorUserId && {
        actorUser: () => renderUser(Number(metadata.actorUserId)),
      }),
      character: renderCharacter,
      ...(metadata.generation && {
        generation: () => renderStrong(metadata.generation),
      }),
      ...(metadata.level && {
        level: () => renderStrong(metadata.level),
      }),
      ...(metadata.gold && {
        gold: () => renderGold(Number(metadata.gold)),
      }),
      ...(metadata.price && {
        price: () => renderGold(Number(metadata.price)),
      }),
      ...(metadata.refundedGold && {
        refundedGold: () => renderGold(Number(metadata.refundedGold)),
      }),
      ...(metadata.heirloomPoints && {
        heirloomPoints: () => renderLoom(Number(metadata.heirloomPoints)),
      }),
      ...(metadata.refundedHeirloomPoints && {
        refundedHeirloomPoints: () => renderLoom(Number(metadata.refundedHeirloomPoints)),
      }),
      ...(metadata.itemId && {
        item: () => renderItem(metadata.itemId),
      }),
      ...(metadata.userItemId && {
        userItem: () => renderItem(metadata.userItemId),
      }),
      ...(metadata.experience && {
        experience: () => renderStrong(n(Number(metadata.experience))),
      }),
      ...(metadata.damage && {
        damage: () => renderDamage(metadata.damage),
      }),
      ...(metadata.instance && {
        instance: () => h(Tag, { variant: 'info', label: metadata.instance }),
      }),
      ...(metadata.gameMode && {
        gameMode: () => h(Tag, { variant: 'info', label: metadata.gameMode }),
      }),
      ...(metadata.oldName && {
        oldName: () => renderStrong(metadata.oldName),
      }),
      ...(metadata.newName && {
        newName: () => renderStrong(metadata.newName),
      }),
      ...(metadata.message && {
        message: () => renderStrong(metadata.message),
      }),
    },
  )
}
</script>

<template>
  <Render />
</template>
