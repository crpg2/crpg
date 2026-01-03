import {
  getBattles as _getBattles,
  deleteBattlesByBattleIdMercenariesByMercenaryId,
  deleteBattlesByBattleIdMercenaryApplications,
  getBattlesByBattleId,
  getBattlesByBattleIdFighterApplications,
  getBattlesByBattleIdFighters,
  getBattlesByBattleIdMercenaries,
  getBattlesByBattleIdMercenaryApplications,
  postBattlesByBattleIdMercenaryApplications,
  putBattlesByBattleIdMercenaryApplicationsByApplicationIdResponse,
  putBattlesByBattleIdSideBriefing,
} from '#api/sdk.gen'

import type { Region } from '~/models/region'
import type { Battle, BattleFighter, BattleFighterApplicationStatus, BattleMercenary, BattleMercenaryApplication, BattleMercenaryApplicationCreation, BattleMercenaryApplicationStatus, BattlePhase, BattleSide, BattleSideBriefing, BattleType } from '~/models/strategus/battle'

import { BATTLE_PHASE, BATTLE_TYPE } from '~/models/strategus/battle'

// need a name
export const SEARCHABLE_BATTLE_PHASE = [
  BATTLE_PHASE.Scheduled,
  BATTLE_PHASE.Hiring,
  BATTLE_PHASE.End,
  BATTLE_PHASE.Live,
]

export const battleIconByType: Record<BattleType, string> = {
  [BATTLE_TYPE.Battle]: 'game-mode-battle',
  [BATTLE_TYPE.Siege]: 'game-mode-conquest',
}

// const battles: Battle[] = [
//   //
//   {
//     id: 1,
//     region: 'Eu',
//     position: {
//       type: 'Point',
//       coordinates: [
//         118.8359375,
//         -89.3515625,
//       ],
//     },
//     phase: 'Scheduled',
//     type: 'Battle',
//     attacker: {
//       id: 23,
//       party: {
//         id: 2,
//         user: {
//           id: 2,
//           platform: 'Steam',
//           platformUserId: '76561198016876889',
//           name: 'orle',
//           avatar: 'https://avatars.steamstatic.com/d51d5155b1a564421c0b3fd5fb7eed7c4474e73d_full.jpg',
//           region: 'Eu',
//           clanMembership: {
//             clan: {
//               id: 1,
//               tag: 'PEC',
//               primaryColor: 4278190318,
//               secondaryColor: 4294957414,
//               name: 'Pecores',
//               description: '',
//               bannerKey: '',
//               region: 'Eu',
//               languages: [
//                 'Fr',
//                 'En',
//               ],
//               discord: null,
//               armoryTimeout: 259200000,
//             },
//             role: 'Leader',
//           },
//         },
//       },
//       settlement: null,
//       side: 'Attacker',
//       commander: true,
//       mercenarySlots: 7,
//     },
//     attackerTotalTroops: 100,
//     defender: {
//       id: 24,
//       party: {
//         id: 52,
//         user: {
//           id: 52,
//           platform: 'Steam',
//           platformUserId: '76561198010855139',
//           name: 'Drexx',
//           avatar: 'https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/ee/ee56a301d3ec686b77c6d06c7517fbb57065b36b_full.jpg',
//           region: 'Eu',
//           clanMembership: null,
//         },
//       },
//       settlement: null,
//       side: 'Defender',
//       commander: true,
//       mercenarySlots: 0,
//     },
//     defenderTotalTroops: 53,
//     createdAt: '2025-12-29T14:58:37.8745074Z',
//     scheduledFor: '2025-12-29T20:00:37.8745072Z',
//   },
// ]

export const getBattles = async (
  region: Region,
  phases: BattlePhase[],
  type?: BattleType,
): Promise<Battle[]> => {
  // return battles
  return (await _getBattles({ query: { region, 'phase[]': phases, type } })).data!
}

export const getBattle = async (
  battleId: number,
) => (await getBattlesByBattleId({ path: { battleId } })).data

export const updateBattleSideBriefing = async (
  battleId: number,
  side: BattleSide,
  briefing: BattleSideBriefing,
) => putBattlesByBattleIdSideBriefing({ path: { battleId }, body: { side, ...briefing } })

// TODO: FIXME:
export const getBattleTitle = (battle: Battle) => {
  // const { t } = useI18n()

  // if (battle.type === 'Siege' && battle.defender?.fighter.settlement) {
  //   return t('strategus.battle.titleByType.Siege', { settlement: battle.defender.fighter.settlement.name })
  // }

  // if (battle.type === 'Battle') {
  //   return t('strategus.battle.titleByType.Battle', {
  //     nearestSettlement: 'nearestSettlement', // TODO: get nearest settlement to point
  //     terrain: 'terrain', // TODO: terrain service get terrain at point
  //   })
  // }

  return 'TODO: FIXME:'
}

export const getBattleFighters = async (
  battleId: number,
): Promise<BattleFighter[]> => (await getBattlesByBattleIdFighters({ path: { battleId } })).data!

export const getBattleFighterByUserId = (
  battleFighters: BattleFighter[],
  userId: number,
) => battleFighters.find(f => (f.party?.user.id || f.settlement?.owner?.id) === userId) || null

export const getBattleFighterApplications = async (
  battleId: number,
  statuses: BattleFighterApplicationStatus[],
) => (await getBattlesByBattleIdFighterApplications({ path: { battleId }, query: { 'status[]': statuses } })).data!

export const getBattleMercenaryApplications = async (
  battleId: number,
  statuses: BattleMercenaryApplicationStatus[],
): Promise<BattleMercenaryApplication[]> => (await getBattlesByBattleIdMercenaryApplications({ path: { battleId }, query: { 'status[]': statuses } })).data!

export const applyToBattleAsMercenary = (
  battleId: number,
  payload: BattleMercenaryApplicationCreation,
) => postBattlesByBattleIdMercenaryApplications({ path: { battleId }, body: payload })

export const respondToBattleMercenaryApplication = (
  battleId: number,
  applicationId: number,
  accept: boolean,
) => putBattlesByBattleIdMercenaryApplicationsByApplicationIdResponse({ path: { battleId, applicationId }, body: { accept } })

export const getBattleMercenaries = async (
  battleId: number,
): Promise<BattleMercenary[]> => (await getBattlesByBattleIdMercenaries({ path: { battleId } })).data!

export const removeBattleMercenary = (
  battleId: number,
  mercenaryId: number,
) => deleteBattlesByBattleIdMercenariesByMercenaryId({ path: { battleId, mercenaryId } })

export const removeBattleMercenaryApplication = (
  battleId: number,
) => deleteBattlesByBattleIdMercenaryApplications({ path: { battleId } })
