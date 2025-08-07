import type { ValueOf } from 'type-fest'

import type { NotificationState as _NotificationState } from '~/api'

export const NOTIFICATION_STATE = {
  Unread: 'Unread',
  Read: 'Read',
} as const satisfies Record<_NotificationState, _NotificationState>

export type NotificationState = ValueOf<typeof NOTIFICATION_STATE>
