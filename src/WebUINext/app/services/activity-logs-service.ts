import { getActivityLogs as _getActivityLogs } from '#hey-api/sdk.gen'

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
    composable: '$fetch',
    query: {
      /* TODO: FIXME:
        contribute https://github.com/hey-api/openapi-ts/blob/897cffa4346f97a80903f8b7df8fb8360a5f4e0f/packages/openapi-ts/src/plugins/%40hey-api/typescript/plugin.ts#L579
        Do not convert to Date if it is a query.
      */

      // @ts-expect-error TODO:
      'to': to.toISOString(),
      // @ts-expect-error TODO:
      'from': from.toISOString(),
      'type[]': types,
      'userId[]': userIds,
    },
  })
  return data
}
