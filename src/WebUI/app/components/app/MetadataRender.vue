<script setup lang="ts">
import { AppCoin, AppLoom, CharacterMedia, ClanRole, ItemThumb, MarketplaceOfferAssetGroup, UBadge, ULink, UserClan, UserMedia, UTooltip } from '#components'
import { I18nT } from 'vue-i18n'

import type { ClanMemberRole } from '~/models/clan'
import type { MarketplaceOfferAsset } from '~/models/marketplace'
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
    ? slots?.user?.({ user }) || h(UserMedia, { user, inline: true, size: 'sm' })
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

const renderGold = (value: number) => h(AppCoin, { value, size: 'lg' })

const renderLoom = (point: number) => h(AppLoom, { point })

const renderBattleLink = (battleId: number) => h(ULink, { to: { name: 'battles-id', params: { id: battleId } } }, { default: () => battleId })

function renderSlots(fields: Record<string, string | number | undefined>, render: (v: string | number) => VNode) {
  return Object.fromEntries(
    Object.entries(fields)
      .filter(([, v]) => v !== undefined)
      .map(([k, v]) => [k, () => render(v!)]),
  )
}

function renderMarketplaceOfferDetails(rawOffered: string, rawRequested: string) {
  try {
    const offered = JSON.parse(rawOffered) as MarketplaceOfferAsset
    const requested = JSON.parse(rawRequested) as MarketplaceOfferAsset
    return h(MarketplaceOfferAssetGroup, { offered, requested })
  }
  catch (_) {
    return renderStrong(JSON.stringify({ offered: rawOffered, requested: rawRequested }))
  }
}

function Render() {
  const {
    clanId,
    oldClanMemberRole,
    newClanMemberRole,
    userId,
    buyerId,
    sellerId,
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
    battleId,
    goldFee,
    listingFee,
    offered,
    requested,
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
      br: () => h('br'),
      ...(offered && requested && { marketplaceOfferDetails: () => renderMarketplaceOfferDetails(offered, requested) }),
      ...(clanId && { clan: () => renderUserClan(Number(clanId)) }),
      ...renderSlots({ oldClanMemberRole, newClanMemberRole }, v => h(ClanRole, { role: v as ClanMemberRole })),
      ...renderSlots({ user: userId, targetUser: targetUserId, actorUser: actorUserId, seller: sellerId, buyer: buyerId }, v => renderUser(Number(v))),
      ...renderSlots({ character: characterId }, v => renderCharacter(Number(v))),
      ...renderSlots({ gold, goldFee, listingFee, price, refundedGold }, v => renderGold(Number(v))),
      ...renderSlots({ heirloomPoints, refundedHeirloomPoints }, v => renderLoom(Number(v))),
      ...renderSlots({ item: itemId, userItem: userItemId }, v => renderItem(String(v))),
      ...(damage && { damage: () => renderDamage(damage) }),
      ...(instance && { instance: () => h(UBadge, { color: 'neutral', size: 'sm', variant: 'subtle', label: instance }) }),
      ...(gameMode && { gameMode: () => h(UBadge, { color: 'neutral', size: 'sm', variant: 'soft', label: gameMode }) }),
      ...(battleId && { battle: () => renderBattleLink(Number(battleId)) }),
      ...Object.entries(restKeys).reduce((out: Record<string, () => VNode>, [key, value]) => {
        const [isNumber, candidateToNumber] = tryGetNumber(value)
        out[key] = () => renderStrong(isNumber ? n(candidateToNumber) : value)
        return out
      }, {} as Record<string, () => any>),
    },
  )
}
</script>

<template>
  <Render />
</template>
