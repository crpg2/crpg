import type { NotificationState as _NotificationState } from '#api'
import type { ValueOf } from 'type-fest'

export const NOTIFICATION_STATE = {
  Unread: 'Unread',
  Read: 'Read',
} as const satisfies Record<_NotificationState, _NotificationState>

export type NotificationState = ValueOf<typeof NOTIFICATION_STATE>
