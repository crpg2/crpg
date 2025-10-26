import { getActivityLogs as _getActivityLogs } from '#api/sdk.gen'

import type { ActivityLog, ActivityLogType } from '~/models/activity-logs'
import type { MetadataDict } from '~/models/metadata'

export interface ActivityLogsPayload {
  to: Date
  from: Date
  userIds: number[]
  types: ActivityLogType[]
}

interface GetActivityLogResponse {
  activityLogs: ActivityLog[]
  dict: MetadataDict
}

export const getActivityLogs = async (
  payload: ActivityLogsPayload,
): Promise<GetActivityLogResponse> => {
  const { to, from, userIds, types } = payload
  const { data } = await _getActivityLogs({
    query: {
      // @ts-expect-error TODO:
      'to': to.toISOString(),
      // @ts-expect-error TODO:
      'from': from.toISOString(),
      'type[]': types,
      'userId[]': userIds,
    },
  })
  return data!
}
