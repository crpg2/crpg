// TODO: FIXME: SPEC
// import type { FetchSpyInstance } from 'vi-fetch'

// import { mockGet } from 'vi-fetch'

// import type { ActivityLog } from '~/models/activity-logs'

// import { response } from '~/__mocks__/crpg-client'
// import { ActivityLogType } from '~/models/activity-logs'

// import { getActivityLogs, getActivityLogsWithUsers } from './activity-logs-service'

// const { mockedGetUsersByIds } = vi.hoisted(() => ({
//   mockedGetUsersByIds: vi.fn().mockResolvedValue([]),
// }))
// vi.mock('~/services/users-service', () => ({ getUsersByIds: mockedGetUsersByIds }))

// beforeEach(() => {
//   vi.useFakeTimers()
//   vi.setSystemTime('2023-03-30T18:00:00.0000000Z')
// })

// afterEach(() => {
//   vi.useRealTimers()
// })

// describe('getActivityLogs', () => {
//   let mock: FetchSpyInstance

//   beforeEach(() => {
//     mock = mockGet(/\/activity-logs/).willDo((url) => {
//       if (url.searchParams.getAll('type[]').length !== 0) {
//         return {
//           body: response([
//             {
//               createdAt: new Date(),
//               id: 1,
//               metadata: {},
//               type: ActivityLogType.UserCreated,
//               userId: 123,
//             },
//           ]),
//         }
//       }

//       if (url.searchParams.getAll('userId[]').length !== 0) {
//         return {
//           body: response([
//             {
//               createdAt: new Date(),
//               id: 1,
//               metadata: {},
//               type: ActivityLogType.UserDeleted,
//               userId: 123,
//             },
//           ]),
//         }
//       }

//       return {
//         body: response([
//           {
//             createdAt: new Date(),
//             id: 1,
//             metadata: {},
//             type: ActivityLogType.UserRenamed,
//             userId: 123,
//           },
//         ]),
//       }
//     })
//   })

//   const payload = {
//     from: new Date('2023-03-22T18:16:42.052359Z'),
//     to: new Date('2023-04-01T18:16:42.052359Z'),
//     userId: [],
//   }

//   it('base', async () => {
//     expect((await getActivityLogs(payload))[0]).toMatchObject({
//       type: ActivityLogType.UserRenamed,
//     })

//     expect(mock).toHaveFetched()
//     expect(mock).toHaveFetchedWithQuery(
//       'from=2023-03-22T18%3A16%3A42.052Z&to=2023-04-01T18%3A16%3A42.052Z',
//     )
//   })

//   it('types', async () => {
//     expect(
//       (await getActivityLogs({ ...payload, type: [ActivityLogType.UserCreated] }))[0],
//     ).toMatchObject({ type: ActivityLogType.UserCreated })

//     expect(mock).toHaveFetched()
//     expect(mock).toHaveFetchedWithQuery(
//       'from=2023-03-22T18%3A16%3A42.052Z&to=2023-04-01T18%3A16%3A42.052Z&type%5B%5D=UserCreated',
//     )
//   })

//   it('userIds', async () => {
//     expect((await getActivityLogs({ ...payload, userId: [123, 124] }))[0]).toMatchObject({
//       type: ActivityLogType.UserDeleted,
//     })

//     expect(mock).toHaveFetched()
//     expect(mock).toHaveFetchedWithQuery(
//       'from=2023-03-22T18%3A16%3A42.052Z&to=2023-04-01T18%3A16%3A42.052Z&userId%5B%5D=123&userId%5B%5D=124',
//     )
//   })
// })

// it('getActivityLogsWithUsers', async () => {
//   mockedGetUsersByIds.mockResolvedValue([{ id: 2 }, { id: 3 }])

//   const logsResponse = [
//     {
//       metadata: {
//         targetUserId: '2',
//       },
//       type: ActivityLogType.TeamHit,
//     },
//     {
//       metadata: {
//         targetUserId: '2',
//       },
//       type: ActivityLogType.TeamHit,
//     },
//     {
//       metadata: {
//         targetUserId: '3',
//       },
//       type: ActivityLogType.TeamHit,
//     },
//     {
//       metadata: {},
//       type: ActivityLogType.ChatMessageSent,
//     },
//   ] as Array<Partial<ActivityLog>>
//   mockGet(/\/activity-logs/).willResolve(response(logsResponse))

//   const result = await getActivityLogsWithUsers({
//     from: new Date(),
//     to: new Date(),
//     userId: [],
//   })

//   expect(mockedGetUsersByIds).toBeCalledWith([2, 3])
//   expect(result.users).toEqual({ 2: { id: 2 }, 3: { id: 3 } })
// })
