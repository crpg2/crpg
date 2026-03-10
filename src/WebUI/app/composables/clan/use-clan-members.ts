import { useAsyncState } from '@vueuse/core'
import { computed } from 'vue'

import type { ClanMemberRole } from '~/models/clan'

import { useClan } from '~/composables/clan/use-clan'
import { useUser } from '~/composables/user/use-user'
import {
  kickClanMember as _kickClanMember,
  updateClanMember as _updateClanMember,
  getClanMembers,
} from '~/services/clan-service'

export const useClanMembers = () => {
  const { clan } = useClan()
  const { user } = useUser()

  const {
    state: clanMembers,
    executeImmediate: loadClanMembers,
    isLoading: loadingClanMembers,
  } = useAsyncState(() => getClanMembers(clan.value.id), [], { resetOnExecute: false })

  const clanMembersCount = computed(() => clanMembers.value.length)

  const isLastMember = computed(() => clanMembersCount.value <= 1)

  const getClanMember = (userId: number) => clanMembers.value.find(m => m.user.id === userId) ?? null

  const updateClanMember = (memberId: number, role: ClanMemberRole) => _updateClanMember(clan.value.id, memberId, role)

  const kickClanMember = (memberId: number) => _kickClanMember(clan.value.id, memberId)

  const selfMember = computed(() => getClanMember(user.value!.id))

  return {
    clanMembers,
    clanMembersCount,
    isLastMember,
    selfMember,
    loadClanMembers,
    loadingClanMembers,
    getClanMember,
    updateClanMember,
    kickClanMember,
  }
}
