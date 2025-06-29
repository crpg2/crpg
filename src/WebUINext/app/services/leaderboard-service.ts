import {
  getLeaderboardLeaderboard,
} from '#hey-api/sdk.gen'
import { inRange } from 'es-toolkit'

import type { CharacterClass } from '~/models/character'
import type {
  CharacterCompetitiveNumbered,
  Rank,
} from '~/models/competitive'
import type { GameMode } from '~/models/game-mode'
import type { Region } from '~/models/region'

import { RankGroup } from '~/models/competitive'
import { getEntries } from '~/utils/object'

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
    composable: '$fetch',
    query: {
      // TODO: FIXME: fix schema
      region: region!,
      gameMode: gameMode!,
      characterClass: characterClass!,
    },
  })

  // TODO: FIXME:
  return data!.map((cr, idx) => ({
    ...cr,
    position: idx + 1,
  }))
}

const rankColors: Record<RankGroup, string> = {
  [RankGroup.Iron]: '#A19D94',
  [RankGroup.Copper]: '#B87333',
  [RankGroup.Bronze]: '#CC6633',
  [RankGroup.Silver]: '#C7CCCA',
  [RankGroup.Gold]: '#EABC40',
  [RankGroup.Platinum]: '#40A7B9',
  [RankGroup.Diamond]: '#C289F5',
  [RankGroup.Champion]: '#B73E6C',
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
  getEntries<Record<RankGroup, string>>(rankColors)
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
