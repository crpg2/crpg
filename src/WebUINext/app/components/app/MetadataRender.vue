<script setup lang="ts">
import { AppCoin, AppLoom, CharacterMedia, ClanRole, ItemThumb, UBadge, UserClan, UserMedia, UTooltip } from '#components'
import { I18nT } from 'vue-i18n'

import type { ClanMemberRole } from '~/models/clan'
import type { MetadataDict } from '~/models/metadata'
import type { UserPublic } from '~/models/user'

import { getItemImage } from '~/services/item-service'

const { keypath, metadata, dict } = defineProps<{
  keypath: string
  metadata: Record<string, string>
  dict: MetadataDict
}>()

defineEmits<{
  read: []
  delete: []
}>()

const slots = defineSlots<{
  user?: (props: { user: UserPublic }) => any
}>()

const { n } = useI18n()

const getClanById = (clanId: number) => dict.clans.find(({ id }) => id === clanId)

const getUserById = (userId: number) => dict.users.find(({ id }) => id === userId)

const getCharacterById = (characterId: number) => dict.characters.find(({ id }) => id === characterId)

const renderStrong = (value: string) => h('strong', { class: 'font-bold text-highlighted' }, value)

const renderDamage = (value: string) => h('strong', { class: 'font-bold text-error' }, n(Number(value)))

const renderUserClan = (clanId: number) => {
  const clan = getClanById(clanId)
  return clan
    ? h(UserClan, { clan })
    : renderStrong(String(clanId))
}

const renderUser = (userId: number) => {
  const user = getUserById(userId)
  return user
    ? slots?.user?.({ user }) || h(UserMedia, { user, size: 'sm' })
    : renderStrong(String(userId))
}

const renderCharacter = (characterId: number) => {
  const character = getCharacterById(Number(characterId))
  return character
    ? h(CharacterMedia, { character })
    : renderStrong(String(characterId))
}

const renderItem = (itemId: string) => {
  return h(
    'span',
    { class: 'inline' },
    h(
      UTooltip,
      { ui: {
        content: 'size-64',
      } },
      {
        default: () => renderStrong(itemId),
        content: () =>
          h(ItemThumb, {
            thumb: getItemImage(itemId),
            name: itemId,
          }),
      },
    ),
  )
}

const renderGold = (value: number) => h(AppCoin, { value })

const renderLoom = (point: number) => h(AppLoom, { point })

function Render() {
  const {
    clanId,
    oldClanMemberRole,
    newClanMemberRole,
    userId,
    targetUserId,
    actorUserId,
    characterId,
    gold,
    price,
    refundedGold,
    heirloomPoints,
    refundedHeirloomPoints,
    itemId,
    userItemId,
    damage,
    instance,
    gameMode,
    ...restKeys
  } = metadata

  return h(
    I18nT,
    {
      tag: 'div',
      scope: 'global',
      keypath,
      class: 'leading-relaxed',
    },
    {
      ...(clanId && { clan: () => renderUserClan(Number(clanId)) }),
      ...(oldClanMemberRole && { oldClanMemberRole: () => h(ClanRole, { role: oldClanMemberRole as ClanMemberRole }) }),
      ...(newClanMemberRole && { newClanMemberRole: () => h(ClanRole, { role: newClanMemberRole as ClanMemberRole }) }),
      ...(userId && { user: () => renderUser(Number(userId)) }),
      ...(targetUserId && { targetUser: () => renderUser(Number(targetUserId)) }),
      ...(actorUserId && { actorUser: () => renderUser(Number(actorUserId)) }),
      ...(characterId && { character: () => renderCharacter(Number(characterId)) }),
      ...(gold && { gold: () => renderGold(Number(gold)) }),
      ...(price && { price: () => renderGold(Number(price)) }),
      ...(refundedGold && { refundedGold: () => renderGold(Number(refundedGold)) }),
      ...(heirloomPoints && { heirloomPoints: () => renderLoom(Number(heirloomPoints)) }),
      ...(refundedHeirloomPoints && { refundedHeirloomPoints: () => renderLoom(Number(refundedHeirloomPoints)) }),
      ...(itemId && { item: () => renderItem(itemId) }),
      ...(userItemId && { userItem: () => renderItem(userItemId) }),
      ...(damage && { damage: () => renderDamage(damage) }),
      ...(instance && { instance: () => h(UBadge, { color: 'neutral', size: 'sm', variant: 'subtle', label: instance }) }),
      ...(gameMode && { gameMode: () => h(UBadge, { color: 'neutral', size: 'sm', variant: 'soft', label: gameMode }) }),
      ...Object.entries(restKeys).reduce((out: Record<string, () => VNode>, [key, value]) => {
        out[key] = () => renderStrong(value)
        return out
      }, {} as Record<string, () => any>),
    },
  )
}
</script>

<template>
  <Render />
</template>
