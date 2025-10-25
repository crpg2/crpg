import type { RestrictionViewModelIListResult, UserPrivateViewModel, UserPublicViewModel } from '~~/generated/api'

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { getRestrictions } from '~/services/restriction-service'

const { _getRestrictionsMock } = vi.hoisted(() => ({
  _getRestrictionsMock: vi.fn(),
}))

vi.mock('#api/sdk.gen', () => ({
  getRestrictions: _getRestrictionsMock,
}))

describe('restriction service', () => {
  describe('getRestrictions', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2022-11-28T22:00:00.000Z'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    it('returns Active when restriction not expired and newest', async () => {
      _getRestrictionsMock.mockResolvedValueOnce({
        data: [
          {
            id: 1,
            type: 'Join',
            createdAt: new Date('2022-11-28T21:59:00.000Z'), // 1 min ago
            duration: 180_000, // 3 min
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 2 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
        ],
        errors: null,
      } satisfies RestrictionViewModelIListResult)

      expect((await getRestrictions()).at(0)?.status).toBe('Active')
    })

    it('returns NonActive when restriction expired', async () => {
      _getRestrictionsMock.mockResolvedValueOnce({
        data: [
          {
            id: 1,
            type: 'Join',
            createdAt: new Date('2022-11-28T21:56:00.000Z'), // 4 min ago
            duration: 180_000, // 3 min
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 2 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
        ],
        errors: null,
      } satisfies RestrictionViewModelIListResult)

      expect((await getRestrictions()).at(0)?.status).toBe('NonActive')
    })

    it('marks older restriction as NonActive if a newer one exists for same user/type', async () => {
      _getRestrictionsMock.mockResolvedValueOnce({
        data: [
          {
            id: 1,
            type: 'Join',
            createdAt: new Date('2022-11-28T21:50:00.000Z'), // 10 min ago
            duration: 180_000, // 3 min
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 2 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
          {
            id: 2,
            type: 'Join',
            createdAt: new Date('2022-11-28T21:59:00.000Z'), // 1 min ago
            duration: 180_000, // 3 min
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 2 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
        ],
        errors: null,
      } satisfies RestrictionViewModelIListResult)

      const [older, newer] = await getRestrictions()
      expect(older?.status).toBe('NonActive')
      expect(newer?.status).toBe('Active')
    })

    it('does not mix restrictions of different users', async () => {
      _getRestrictionsMock.mockResolvedValueOnce({
        data: [
          {
            id: 1,
            type: 'Join',
            createdAt: new Date('2022-11-28T21:59:00.000Z'),
            duration: 180_000,
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 2 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
          {
            id: 2,
            type: 'Join',
            createdAt: new Date('2022-11-28T21:59:00.000Z'),
            duration: 180_000,
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 3 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
        ],
        errors: null,
      } satisfies RestrictionViewModelIListResult)

      const [first, second] = await getRestrictions()
      expect(first?.status).toBe('Active')
      expect(second?.status).toBe('Active')
    })

    it('does not mix restrictions of different types', async () => {
      _getRestrictionsMock.mockResolvedValueOnce({
        data: [
          {
            id: 1,
            type: 'Join',
            createdAt: new Date('2022-11-28T21:59:00.000Z'),
            duration: 180_000,
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 2 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
          {
            id: 2,
            type: 'Chat',
            createdAt: new Date('2022-11-28T21:59:00.000Z'),
            duration: 180_000,
            restrictedByUser: { id: 1 } as UserPublicViewModel,
            restrictedUser: { id: 2 } as UserPrivateViewModel,
            reason: '',
            publicReason: '',
          },
        ],
        errors: null,
      } satisfies RestrictionViewModelIListResult)

      const [first, second] = await getRestrictions()
      expect(first?.status).toBe('Active')
      expect(second?.status).toBe('Active')
    })
  })
})
