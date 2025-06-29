import {
  putUsersByIdRewards,
  putUsersByUserIdCharactersByCharacterIdRewards,
} from '#hey-api/sdk.gen'

interface RewardUser {
  gold: number
  heirloomPoints: number
  itemId: string
}
export const rewardUser = (
  userId: number,
  payload: RewardUser,
) => putUsersByIdRewards({ composable: '$fetch', path: { id: userId }, body: payload })

interface RewardCharacter {
  experience: number
  autoRetire: boolean
}
export const rewardCharacter = (
  userId: number,
  characterId: number,
  payload: RewardCharacter,
) => putUsersByUserIdCharactersByCharacterIdRewards({ composable: '$fetch', path: { userId, characterId }, body: payload })
