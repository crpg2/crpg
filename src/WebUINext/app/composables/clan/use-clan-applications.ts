import { useAsyncState } from '@vueuse/core'

import { CLAN_INVITATION_STATUS, CLAN_INVITATION_TYPE } from '~/models/clan'
import {
  inviteToClan as _inviteToClan,
  respondToClanInvitation as _respondToClanInvitation,
  getClanInvitations,
} from '~/services/clan-service'

export const useClanApplications = (clanId: MaybeRefOrGetter<number>) => {
  const {
    state: applications,
    execute: loadClanApplications,
    isLoading: loadingClanApplications,
  } = useAsyncState(
    () => getClanInvitations(toValue(clanId), [CLAN_INVITATION_TYPE.Request], [CLAN_INVITATION_STATUS.Pending]),
    [],
    {
      immediate: false,
      resetOnExecute: false,
    },
  )

  const respondToClanInvitation = (invitationId: number, accept: boolean) => _respondToClanInvitation(toValue(clanId), invitationId, accept)

  const applicationsCount = computed(() => applications.value.length)

  const inviteToClan = (inviteeId: number) => _inviteToClan(toValue(clanId), inviteeId)

  return {
    applications,
    applicationsCount,
    loadClanApplications,
    loadingClanApplications,
    respondToClanInvitation,
    inviteToClan,
  }
}
