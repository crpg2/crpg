// import { mockGet, mockPost } from 'vi-fetch'

// import type { Restriction, RestrictionCreation } from '~/models/restriction'
// import type { UserPrivate, UserPublic } from '~/models/user'

// import { response } from '~/__mocks__/crpg-client'
// import { RestrictionType } from '~/models/restriction'
// import { getRestrictions, mapRestrictions, restrictUser } from '~/services/restriction-service'

// const { mockCheckIsDateExpired } = vi.hoisted(() => ({
//   mockCheckIsDateExpired: vi.fn(),
// }))
// vi.mock('~/utils/date', () => ({
//   checkIsDateExpired: mockCheckIsDateExpired,
// }))

// const duration = 180000 // 3 min

// const createRestriction = (payload: Partial<Restriction> = {}): Restriction => ({
//   createdAt: new Date('2022-11-27T22:00:00.000Z'),
//   duration,
//   id: 1,
//   publicReason: '',
//   reason: '',
//   restrictedByUser: {} as UserPublic,
//   restrictedUser: { id: 1 } as UserPrivate,
//   type: RestrictionType.Join,
//   ...payload,

// })

// describe('mapRestrictions', () => {
//   describe('single', () => {
//     const payload: Restriction[] = [createRestriction(),
//     ]

//     it('non-expired', () => {
//       mockCheckIsDateExpired.mockReturnValue(false)

//       expect(mapRestrictions(payload).at(0)?.active).toBeTruthy()
//     })

//     it('expired', () => {
//       mockCheckIsDateExpired.mockReturnValue(true)

//       expect(mapRestrictions(payload).at(0)?.active).toBeFalsy()
//     })
//   })

//   describe('several', () => {
//     describe('same type', () => {
//       const payload = [
//         createRestriction({
//           createdAt: new Date('2022-11-28T22:00:00.000Z'),
//           id: 1,
//           restrictedUser: { id: 1 } as UserPrivate,
//           type: RestrictionType.Join,
//         }),
//         createRestriction({
//           createdAt: new Date('2022-11-27T22:00:00.000Z'),
//           id: 2,
//           restrictedUser: { id: 1 } as UserPrivate,
//           type: RestrictionType.Join,
//         }),
//       ]

//       it('non-expired', () => {
//         mockCheckIsDateExpired.mockReturnValue(false)

//         const result = mapRestrictions(payload)

//         expect(result.at(0)?.active).toBeTruthy()
//         expect(result.at(1)?.active).toBeFalsy()
//       })

//       it('expired', () => {
//         mockCheckIsDateExpired.mockReturnValue(true)

//         const result = mapRestrictions(payload)

//         expect(result.at(0)?.active).toBeFalsy()
//         expect(result.at(1)?.active).toBeFalsy()
//       })
//     })

//     describe('different type', () => {
//       const payload = [
//         createRestriction({
//           type: RestrictionType.Chat,
//         }),
//         createRestriction({
//           type: RestrictionType.Join,
//         }),
//       ]

//       it('non-expired', () => {
//         mockCheckIsDateExpired.mockReturnValue(false)

//         const result = mapRestrictions(payload)

//         expect(result.at(0)?.active).toBeTruthy()
//         expect(result.at(1)?.active).toBeTruthy()
//       })

//       it('expired', () => {
//         mockCheckIsDateExpired.mockReturnValue(true)

//         const result = mapRestrictions(payload)

//         expect(result.at(0)?.active).toBeFalsy()
//         expect(result.at(1)?.active).toBeFalsy()
//       })
//     })

//     describe('different user', () => {
//       const payload = [
//         createRestriction({
//           restrictedUser: { id: 1 } as UserPrivate,
//         }),
//         createRestriction({
//           restrictedUser: { id: 2 } as UserPrivate,
//         }),
//       ]

//       it('non-expired', () => {
//         mockCheckIsDateExpired.mockReturnValue(false)

//         const result = mapRestrictions(payload)

//         expect(result.at(0)?.active).toBeTruthy()
//         expect(result.at(1)?.active).toBeTruthy()
//       })

//       it('expired', () => {
//         mockCheckIsDateExpired.mockReturnValue(true)

//         const result = mapRestrictions(payload as Restriction[])

//         expect(result.at(0)?.active).toBeFalsy()
//         expect(result.at(1)?.active).toBeFalsy()
//       })
//     })
//   })
// })

// it('getRestrictions', async () => {
//   mockCheckIsDateExpired.mockReturnValue(false)
//   const restrictions = {
//     duration,
//     id: 1,
//     reason: '',
//     restrictedByUser: { id: 1 },
//     restrictedUser: { id: 1 },
//     type: 'Join',
//   }

//   mockGet('/restrictions').willResolve(response([restrictions]))
//   expect(await getRestrictions()).toEqual([{ ...restrictions, active: true }])

//   expect(mockCheckIsDateExpired).toBeCalled()
// })

// it('restrictUser', async () => {
//   const payload = {
//     duration: 100,
//     reason: '',
//     restrictedUserId: 1,
//     type: 'Chat',
//   } as RestrictionCreation

//   const restriction = {
//     duration: 1,
//     id: 1,
//     reason: '',
//     restrictedByUser: { id: 1 },
//     restrictedUser: { id: 1 },
//     type: 'Join',
//   }

//   const mock = mockPost('/restrictions').willResolve(response(restriction))
//   expect(await restrictUser(payload)).toEqual(restriction)
//   expect(mock).toHaveFetchedWithBody(payload)
// })
