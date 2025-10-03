import { getLeaderboardLeaderboard } from '#api/sdk.gen'
import { inRange } from 'es-toolkit'

import type { CharacterClass } from '~/models/character'
import type {
  CharacterCompetitiveNumbered,
  Rank,
  RankGroup,
} from '~/models/competitive'
import type { GameMode } from '~/models/game-mode'
import type { Region } from '~/models/region'

import { RANK_GROUP } from '~/models/competitive'

export const getLeaderBoard = async ({
  characterClass,
  gameMode,
  region,
}: {
  region?: Region
  characterClass?: CharacterClass
  gameMode?: GameMode
}): Promise<CharacterCompetitiveNumbered[]> => {
  const { data } = await getLeaderboardLeaderboard({
    query: {
      region: region!,
      gameMode: gameMode!,
      characterClass: characterClass!,
    },
  })

  return data!.map((item, idx) => ({
    ...item,
    position: idx + 1,
  }))
}

const rankColors: Record<RankGroup, string> = {
  [RANK_GROUP.Iron]: '#A19D94',
  [RANK_GROUP.Copper]: '#B87333',
  [RANK_GROUP.Bronze]: '#CC6633',
  [RANK_GROUP.Silver]: '#C7CCCA',
  [RANK_GROUP.Gold]: '#EABC40',
  [RANK_GROUP.Platinum]: '#40A7B9',
  [RANK_GROUP.Diamond]: '#C289F5',
  [RANK_GROUP.Champion]: '#B73E6C',
}

const step = 50
const rankSubGroupCount = 5

const createRank = (baseRank: [RankGroup, string]) =>
  [...Array.from({ length: rankSubGroupCount }).keys()].reverse().map(subRank => ({
    color: baseRank[1],
    groupTitle: baseRank[0],
    title: `${baseRank[0]} ${subRank + 1}`,
  }))

export const createRankTable = (): Rank[] =>
  objectEntries<Record<RankGroup, string>>(rankColors)
    .flatMap(createRank)
    .map((baseRank, idx) => ({ ...baseRank, max: idx * step + step, min: idx * step }))

export const getRankByCompetitiveValue = (rankTable: Rank[], competitiveValue: number) => {
  const first = rankTable.at(0)
  const last = rankTable.at(-1)

  if (first && competitiveValue < first.min) {
    return first
  }

  if (last && competitiveValue > last.max) {
    return last
  }

  return createRankTable().find(row => inRange(competitiveValue, row.min, row.max))!
}
