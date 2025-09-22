// import {
//   defaultExperienceMultiplier,
//   newUserStartingCharacterLevel,
// } from '~root/data/constants.json'

// import { getCharacters } from '~/services/character-service'
import {
  // buyUserItem,
  getUser,
  // getUserItems,
  getUserRestriction,
} from '~/services/user-service'

// TODO: add userProvider
export const useUserStore = defineStore('user', () => {
  const {
    state: user,
    execute: fetchUser,
  } = useAsyncState(
    () => getUser(),
    null,
    { resetOnExecute: false, immediate: false },
  )

  // const {
  //   state: characters,
  //   execute: fetchCharacters,
  //   isLoading: fetchingCharacters,
  // } = useAsyncState(
  //   () => getCharacters(),
  //   [],
  //   { resetOnExecute: false, immediate: false },
  // )

  // const {
  //   state: userItems,
  //   execute: fetchUserItems,
  // } = useAsyncState(
  //   () => getUserItems(),
  //   [],
  //   { resetOnExecute: false, immediate: false },
  // )

  // const activeCharacterId = computed(() => user.value?.activeCharacterId || characters.value?.[0]?.id || null)

  // const validateCharacter = (id: number) => characters.value.some(c => c.id === id)

  // const getCurrentCharacterById = (characterId: number) => {
  //   const character = characters.value.find(c => c.id === characterId)
  //   if (!character) {
  //     throw new Error('character not found')
  //   }
  //   return character
  // }

  // // TODO: mby to backend?
  // const isRecentUser = computed(() => {
  //   if (user.value === null || characters.value.length === 0) {
  //     return false
  //   }

  //   const hasHighLevelCharacter = characters.value.some(
  //     c => c.level > newUserStartingCharacterLevel,
  //   )
  //   const totalExperience = characters.value.reduce((total, c) => total + c.experience, 0)
  //   const wasRetired = user.value.experienceMultiplier !== defaultExperienceMultiplier

  //   return (
  //     !hasHighLevelCharacter
  //     && !wasRetired
  //     && totalExperience < 12000000 // protection against abusers of free re-specialization mechanics
  //   )
  // })

  const hasUnreadNotifications = computed(() => Boolean(user.value?.unreadNotificationsCount))

  // TODO:
  const {
    state: restriction,
    execute: fetchUserRestriction,
  } = useAsyncState(
    () => getUserRestriction(),
    null,
    { resetOnExecute: false, immediate: false },
  )

  // const buyItem = async (itemId: string) => {
  //   await buyUserItem(itemId)
  //   // Promise.all([fetchUserItems(), fetchUser()]) //
  // }

  return {
    user,
    fetchUser,
    // isRecentUser,

    clan: toRef(() => user.value?.clanMembership?.clan ?? null),
    clanMemberRole: toRef(() => user.value?.clanMembership?.role || null),

    // characters,
    // fetchCharacters,
    // fetchingCharacters,
    // activeCharacterId,
    // validateCharacter,
    // getCurrentCharacterById,

    // userItems,
    // fetchUserItems,

    // buyItem,

    restriction,
    fetchUserRestriction,

    hasUnreadNotifications,
  }
})
