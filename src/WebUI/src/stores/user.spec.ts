import { createPinia, setActivePinia } from 'pinia'

import type { Character } from '~/models/character'

import mockCharacters from '~/__mocks__/characters.json'
import mockUserItems from '~/__mocks__/user-items.json'
import mockUser from '~/__mocks__/user.json'

import { useUserStore } from './user'

const { mockedGetUserClan } = vi.hoisted(() => ({ mockedGetUserClan: vi.fn() }))
vi.mock('~/services/users-service', () => {
  return {
    buyUserItem: vi.fn().mockResolvedValue(mockUserItems[0]),
    getUser: vi.fn().mockResolvedValue(mockUser),
    getUserClan: mockedGetUserClan,
    getUserItems: vi.fn().mockResolvedValue(mockUserItems),
  }
})

vi.mock('~/services/characters-service', () => {
  return {
    getCharacters: vi.fn().mockResolvedValue(mockCharacters),
  }
})

describe('userStore', () => {
  let store: ReturnType<typeof useUserStore>

  beforeEach(() => {
    setActivePinia(createPinia())
    store = useUserStore()
  })

  it('references a store', () => {
    expect(store).toBeDefined()
  })

  it('has default state on init', () => {
    expect(store.user).toEqual(null)
    expect(store.characters).toEqual([])
  })

  describe('getters: activeCharacterId', () => {
    it('user.activeCharacterId', () => {
      store.$patch({ characters: [{ id: 1 }, { id: 112 }], user: { activeCharacterId: 112 } })

      expect(store.activeCharacterId).toEqual(112)
    })

    it('characters[0].id', () => {
      store.$patch({ characters: [{ id: 1 }, { id: 112 }] })

      expect(store.activeCharacterId).toEqual(1)
    })

    it('non active char', () => {
      expect(store.activeCharacterId).toEqual(null)
    })
  })

  describe('actions', () => {
    it('fetchUser', async () => {
      await store.fetchUser()

      expect(store.user).toEqual(mockUser)
    })

    it('fetchCharacters', async () => {
      await store.fetchCharacters()

      expect(store.characters).toEqual(mockCharacters)
    })

    it('validateCharacter', () => {
      store.$patch({ characters: [{ id: 1 }, { id: 112 }] })
      expect(store.validateCharacter(1)).toBeTruthy()
      expect(store.validateCharacter(112)).toBeTruthy()
      expect(store.validateCharacter(223)).toBeFalsy()
    })

    it('replaceCharacter', () => {
      store.$patch({
        characters: [
          { id: 1, name: 'Rarity' },
          { id: 112, name: 'Spike' },
        ],
      })

      store.replaceCharacter({ id: 112, name: 'Twilight Sparkle' } as Character)

      expect(store.characters).toEqual([
        { id: 1, name: 'Rarity' },
        { id: 112, name: 'Twilight Sparkle' },
      ])
    })

    it('fetchUserItems', async () => {
      await store.fetchUserItems()

      expect(store.userItems).toEqual(mockUserItems)
    })

    it('subtractGold', () => {
      store.$patch({ user: { gold: 100 } })

      store.subtractGold(50)

      expect(store.user!.gold).toEqual(50)
    })

    it('buyItem', async () => {
      store.$patch({ user: { gold: 100 } })

      await store.buyItem('id123')

      expect(store.userItems).toEqual([mockUserItems[0]])
    })
  })
})
