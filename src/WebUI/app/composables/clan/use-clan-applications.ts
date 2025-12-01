import { useAsyncState } from '@vueuse/core'
import { computed } from 'vue'

import {
  inviteToClan as _inviteToClan,
  respondToClanInvitation as _respondToClanInvitation,
  getClanInvitations,
} from '~/services/clan-service'

import { useClan } from './use-clan'

export const useClanApplications = (immediate = true) => {
  const { clan } = useClan()

  const {
    state: applications,
    executeImmediate: loadClanApplications,
    isLoading: loadingClanApplications,
  } = useAsyncState(() => getClanInvitations(clan.value.id), [], { immediate, resetOnExecute: false })

  const applicationsCount = computed(() => applications.value.length)

  const respondToClanInvitation = (invitationId: number, accept: boolean) => _respondToClanInvitation(clan.value.id, invitationId, accept)

  const inviteToClan = (inviteeId: number) => _inviteToClan(clan.value.id, inviteeId)

  return {
    applications,
    applicationsCount,
    loadClanApplications,
    loadingClanApplications,
    respondToClanInvitation,
    inviteToClan,
  }
}
