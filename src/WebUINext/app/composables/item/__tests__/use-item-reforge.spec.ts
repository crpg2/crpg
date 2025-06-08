import { createTestingPinia } from '@pinia/testing'

import type { ItemFlat } from '~/models/item'

import { useUserStore } from '~/stores/user'

import { useItemReforge } from '../use-item-reforge'

const { mockedReforgeCostByRank } = vi.hoisted(() => ({
  mockedReforgeCostByRank: {
    0: 0,
    1: 40000,
    2: 90000,
    3: 150000,
  },
}))
vi.mock('~/services/item-service', () => ({
  reforgeCostByRank: mockedReforgeCostByRank,
}))

const userStore = useUserStore(createTestingPinia())

beforeEach(() => {
  userStore.$reset()
  userStore.$patch({ user: { gold: 1000 } })
})

const item = {
  baseId: 'test-item',
  id: 'test-item-h2',
  rank: 2,
} as ItemFlat

it('reforgeCost', () => {
  const { reforgeCost } = useItemReforge(item)

  expect(reforgeCost.value).toEqual(mockedReforgeCostByRank[item.rank])
})

it('reforgeCostTable - table without 0 item', () => {
  const { reforgeCostTable } = useItemReforge(item)

  expect(reforgeCostTable.value.length).toEqual(3)
})

describe('validation', () => {
  it('not much gold', () => {
    userStore.$patch({ user: { gold: 1000 } })

    const { validation } = useItemReforge(item)

    expect(validation.value.rank).toBeTruthy()
    expect(validation.value.gold).toBeFalsy()
  })

  it('lots of gold', () => {
    userStore.$patch({ user: { gold: 500000 } })

    const { validation } = useItemReforge(item)

    expect(validation.value.gold).toBeTruthy()
  })

  it('you cannot reforge an base item', () => {
    userStore.$patch({ user: { gold: 500000 } })

    const { validation } = useItemReforge({
      baseId: 'test-item',
      id: 'test-item-h2',
      rank: 0,
    } as ItemFlat)

    expect(validation.value.rank).toBeFalsy()
  })

  it('canReforge', () => {
    userStore.$patch({ user: { gold: 500000 } })

    const { canReforge } = useItemReforge(item)

    expect(canReforge.value).toBeTruthy()
  })
})
