import type { ClanMemberRole } from '~/models/clan'

import {
  kickClanMember as _kickClanMember,
  updateClanMember as _updateClanMember,
  getClanMembers,
} from '~/services/clan-service'

export const useClanMembers = (clanId: MaybeRefOrGetter<number>) => {
  const {
    state: clanMembers,
    execute: loadClanMembers,
    isLoading: loadingClanMembers,
  } = useAsyncState(
    () => getClanMembers(toValue(clanId)),
    [],
    {
      immediate: false,
    },
  )

  const clanMembersCount = computed(() => clanMembers.value.length)

  const isLastMember = computed(() => clanMembersCount.value <= 1)

  const getClanMember = (userId: number) => clanMembers.value.find(m => m.user.id === userId) || null

  const updateClanMember = (memberId: number, role: ClanMemberRole) => _updateClanMember(toValue(clanId), memberId, role)

  const kickClanMember = (memberId: number) => _kickClanMember(toValue(clanId), memberId)

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
