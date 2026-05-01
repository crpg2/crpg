import { describe, expect, it, vi } from 'vitest'

import type { QuestDefinition } from '~/models/quest'

import { GAME_EVENT_FIELD, GAME_EVENT_TYPE, QUEST_AGGREGATION_TYPE, QUEST_TYPE } from '~/models/quest'

import { useQuestTitle } from '../use-quest-title'

const { mockedT, mockedN } = vi.hoisted(() => ({
  mockedT: vi.fn((key: string, params?: Record<string, unknown>) => params ? `${key}(${JSON.stringify(params)})` : key),
  mockedN: vi.fn((value: number) => String(value)),
}))

vi.mock('#imports', () => ({
  useI18n: vi.fn(() => ({ t: mockedT, n: mockedN })),
}))

vi.mock('#components', () => ({
  UBadge: { name: 'UBadge' },
}))

vi.mock('~/services/item-service', () => ({
  weaponClassToIcon: {
    OneHandedSword: 'one-handed-sword',
    TwoHandedSword: 'two-handed-sword',
    OneHandedAxe: 'one-handed-axe',
    TwoHandedAxe: 'two-handed-axe',
    OneHandedMace: 'one-handed-mace',
    TwoHandedMace: 'two-handed-mace',
    Dagger: 'dagger',
    OneHandedPolearm: 'one-handed-polearm',
    TwoHandedPolearm: 'two-handed-polearm',
    Bow: 'bow',
    Crossbow: 'crossbow',
    Thrown: 'thrown',
    Shield: 'shield',
    Banner: 'banner',
  },
}))

function makeDefinition(overrides: Partial<QuestDefinition> = {}): QuestDefinition {
  return {
    id: 1,
    type: QUEST_TYPE.Daily,
    eventType: GAME_EVENT_TYPE.Kill,
    aggregationType: QUEST_AGGREGATION_TYPE.Count,
    aggregationField: null,
    eventFiltersJson: [],
    requiredValue: 10,
    rewardGold: 100,
    rewardExperience: 200,
    ...overrides,
  }
}

describe('useQuestTitle', () => {
  it('renders title with hitType using the hitType-specific key', () => {
    const render = useQuestTitle(makeDefinition({
      eventFiltersJson: [{ [GAME_EVENT_FIELD.HitType]: 'Melee' }],
    }))
    render()

    expect(mockedT).toHaveBeenCalledWith(
      `user.quests.generate.tplByEventTypeAndAggregationTypeAndHitType.Kill_Count.Melee`,
      { value: '10' },
    )
  })

  it('renders body part badges with "to" connector', () => {
    const render = useQuestTitle(makeDefinition({
      eventFiltersJson: [{ [GAME_EVENT_FIELD.BodyPart]: 'Head' }],
    }))
    const vnode = render()

    expect(mockedT).toHaveBeenCalledWith('user.quests.generate.titleConnector.to')
    expect(mockedT).toHaveBeenCalledWith('user.quests.generate.eventField.BodyPart.values.Head')
    expect(vnode.children).toHaveLength(3) // title span + connector span + badge
  })

  it('renders weapon class badges with "with" connector', () => {
    const render = useQuestTitle(makeDefinition({
      eventFiltersJson: [{ [GAME_EVENT_FIELD.WeaponClass]: 'Bow' }],
    }))
    const vnode = render()

    expect(mockedT).toHaveBeenCalledWith('user.quests.generate.titleConnector.with')
    expect(mockedT).toHaveBeenCalledWith('item.weaponClass.Bow')
    expect(vnode.children).toHaveLength(3) // title span + connector span + badge
  })

  it('renders both body part and weapon class badges', () => {
    const render = useQuestTitle(makeDefinition({
      eventFiltersJson: [
        { [GAME_EVENT_FIELD.BodyPart]: 'Head' },
        { [GAME_EVENT_FIELD.WeaponClass]: 'Bow' },
      ],
    }))
    const vnode = render()
    // title + "to" + body badge + "with" + weapon badge
    expect(vnode.children).toHaveLength(5)
  })
})
