import qs from 'qs'

import type { ActivityLog, ActivityLogType } from '~/models/activity-logs'
import type { MetadataDict } from '~/models/metadata'

import { get } from '~/services/crpg-client'

export interface ActivityLogsPayload {
  to: Date
  from: Date
  userId: number[]
  type?: ActivityLogType[]
}

export const getActivityLogs = async (payload: ActivityLogsPayload) =>
  get<{ activityLogs: ActivityLog[], dict: MetadataDict }>(
    `/activity-logs?${qs.stringify(payload, {
      arrayFormat: 'brackets',
      skipNulls: true,
    })}`,
  )
