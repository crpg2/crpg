import type { ClanMemberRole } from '~/models/clan'

import {
  kickClanMember as _kickClanMember,
  updateClanMember as _updateClanMember,
  getClanMembers,
} from '~/services/clan-service'

import { useClan } from './use-clan'

export const useClanMembers = () => {
  const { clan } = useClan()

  const {
    state: clanMembers,
    execute: loadClanMembers,
    isLoading: loadingClanMembers,
  } = useAsyncState(() => getClanMembers(clan.value.id), [], { resetOnExecute: false })

  const clanMembersCount = computed(() => clanMembers.value.length)

  const isLastMember = computed(() => clanMembersCount.value <= 1)

  const getClanMember = (userId: number) => clanMembers.value.find(m => m.user.id === userId) ?? null

  const updateClanMember = (memberId: number, role: ClanMemberRole) => _updateClanMember(clan.value.id, memberId, role)

  const kickClanMember = (memberId: number) => _kickClanMember(clan.value.id, memberId)

  return {
    clanMembers,
    clanMembersCount,
    isLastMember,
    loadClanMembers,
    loadingClanMembers,
    getClanMember,
    updateClanMember,
    kickClanMember,
  }
}
