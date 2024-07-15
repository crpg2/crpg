import qs from 'qs'

import type {
  ActivityLog,
  ActivityLogMetadataDicts,
  ActivityLogType,
} from '~/models/activity-logs'

import { get } from '~/services/crpg-client'

export interface ActivityLogsPayload {
  to: Date
  from: Date
  userId: number[]
  type?: ActivityLogType[]
}

export const getActivityLogs = async (payload: ActivityLogsPayload) =>
  get<{ activityLogs: ActivityLog[], dict: ActivityLogMetadataDicts }>(
    `/activity-logs?${qs.stringify(payload, {
      arrayFormat: 'brackets',
      skipNulls: true,
    })}`,
  )
