import {
  defaultExperienceMultiplier,
  newUserStartingCharacterLevel,
} from '~root/data/constants.json'

import type { Character } from '~/models/character'
import type { Clan, ClanMemberRole } from '~/models/clan'
import type { PublicRestriction } from '~/models/restriction'
import type { User, UserItem } from '~/models/user'

import { getCharacters } from '~/services/characters-service'
import {
  buyUserItem,
  getUser,
  getUserClan,
  getUserItems,
  getUserRestriction,
} from '~/services/users-service'

interface State {
  user: User | null
  clan: Clan | null
  userItems: UserItem[]
  characters: Character[]
  clanMemberRole: ClanMemberRole | null
  restriction: PublicRestriction | null
}

export const useUserStore = defineStore('user', {

  state: (): State => ({
    characters: [],
    clan: null,
    clanMemberRole: null,
    restriction: null,
    user: null,
    userItems: [],
  }),

  getters: {
    hasUnreadNotifications: state =>
      state.user?.unreadNotificationsCount !== undefined
        ? state.user?.unreadNotificationsCount > 0
        : false,

    activeCharacterId: state => state.user?.activeCharacterId || state.characters?.[0]?.id || null,

    isRecentUser: (state) => {
      if (state.user === null || state.characters.length === 0) {
        return false
      }

      // TODO: SPEC
      // mby to service?
      const hasHighLevelCharacter = state.characters.some(
        c => c.level > newUserStartingCharacterLevel,
      )
      const totalExperience = state.characters.reduce((total, c) => total + c.experience, 0)
      const wasRetired = state.user.experienceMultiplier !== defaultExperienceMultiplier

      return (
        !hasHighLevelCharacter
        && !wasRetired
        //
        && totalExperience < 12000000 // protection against abusers of free re-specialization mechanics
      )
    },
  },

  actions: {
    addUserItem(userItem: UserItem) {
      this.userItems.push(userItem)
    },

    async buyItem(itemId: string) {
      const userItem = await buyUserItem(itemId)
      this.addUserItem(userItem)
      this.subtractGold(userItem.item.price)
    },

    async fetchCharacters() {
      this.characters = await getCharacters()
    },

    async fetchUser() {
      this.user = await getUser()
    },

    async fetchUserClanAndRole() {
      const userClanAndRole = await getUserClan()

      if (userClanAndRole === null) {
        this.clan = null
        this.clanMemberRole = null
        return
      }

      this.clan = userClanAndRole.clan
      this.clanMemberRole = userClanAndRole.role
    },

    async fetchUserItems() {
      this.userItems = await getUserItems()
    },

    async fetchUserRestriction() {
      this.restriction = await getUserRestriction()
    },

    replaceCharacter(character: Character) {
      this.characters.splice(
        this.characters.findIndex(c => c.id === character.id),
        1,
        character,
      )
    },

    subtractGold(loss: number) {
      this.user!.gold -= loss
    },

    validateCharacter(id: number) {
      return this.characters.some(c => c.id === id)
    },
  },
})
