import {
  getUsersSelfQuests,
  putUsersSelfQuestsByIdClaim,
  putUsersSelfQuestsByIdReroll,
} from '#api/sdk.gen'

import type { GameEventField, UserQuest } from '~/models/quest'

export const getUserQuests = async (): Promise<UserQuest[]> => {
  const data = (await getUsersSelfQuests({})).data!

  return data.map((item) => {
    const { questDefinition, ...rest } = item
    const { eventFiltersJson, ...questDefinitionRest } = questDefinition

    return ({
      ...rest,
      questDefinition: {
        ...questDefinitionRest,
        eventFiltersJson: eventFiltersJson as Record<GameEventField, string>[],
      },
    })
  })
}

export const claimQuestReward = (questId: number, characterId: number) =>
  putUsersSelfQuestsByIdClaim({ path: { id: questId }, body: { characterId } })

export const rerollQuest = (questId: number) =>
  putUsersSelfQuestsByIdReroll({ path: { id: questId } })
