import {
  defaultExperienceMultiplier,
  newUserStartingCharacterLevel,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { UserItem } from '~/models/user'

import { useAsyncCallback } from '~/composables/utils/use-async-callback'
import { getCharacters } from '~/services/characters-service'
import {
  buyUserItem,
  getUser,
  getUserItems,
  getUserRestriction,
} from '~/services/users-service'

export const useUserStore = defineStore('user', () => {
  const { state: user, execute: fetchUser } = useAsyncState(() => getUser(), null, { resetOnExecute: false, immediate: false })

  const { state: characters, execute: fetchCharacters } = useAsyncState(() => getCharacters(), [], { resetOnExecute: false, immediate: false })

  const { state: userItems, execute: fetchUserItems } = useAsyncState(() => getUserItems(), [], { resetOnExecute: false, immediate: false })

  const activeCharacterId = computed(() => user.value?.activeCharacterId || characters.value?.[0]?.id || null)

  const validateCharacter = (id: number) => {
    return characters.value.some(c => c.id === id)
  }

  const replaceCharacter = (character: Character) => {
    characters.value.splice(
      characters.value.findIndex(c => c.id === character.id),
      1,
      character,
    )
  }

  // TODO: mby to backend?
  const isRecentUser = computed(() => {
    if (user.value === null || characters.value.length === 0) {
      return false
    }

    const hasHighLevelCharacter = characters.value.some(
      c => c.level > newUserStartingCharacterLevel,
    )
    const totalExperience = characters.value.reduce((total, c) => total + c.experience, 0)
    const wasRetired = user.value.experienceMultiplier !== defaultExperienceMultiplier

    return (
      !hasHighLevelCharacter
      && !wasRetired
      && totalExperience < 12000000 // protection against abusers of free re-specialization mechanics
    )
  })

  const hasUnreadNotifications = computed(() => Boolean(user.value?.unreadNotificationsCount))

  const { state: restriction, execute: fetchUserRestriction } = useAsyncState(() => getUserRestriction(), null, { resetOnExecute: false, immediate: false })

  const subtractGold = (loss: number) => {
    if (!user.value) { return }
    user.value.gold -= loss
  }

  const addUserItem = (userItem: UserItem) => {
    userItems.value.push(userItem)
  }

  const { execute: buyItem, loading: buyingItem } = useAsyncCallback(async (itemId: string) => {
    const userItem = await buyUserItem(itemId)
    addUserItem(userItem)
    subtractGold(userItem.item.price)
  })

  return {
    user,
    fetchUser,
    isRecentUser,
    subtractGold,

    clan: toRef(() => user.value?.clanMembership?.clan || null),
    clanMemberRole: toRef(() => user.value?.clanMembership?.role || null),

    characters,
    fetchCharacters,
    activeCharacterId,
    validateCharacter,
    replaceCharacter,

    userItems,
    fetchUserItems,

    buyItem,
    buyingItem,

    restriction,
    fetchUserRestriction,

    hasUnreadNotifications,
  }
})
