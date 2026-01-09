import type { NotificationState as _NotificationState, NotificationType as _NotificationType } from '#api'
import type { ValueOf } from 'type-fest'

export const NOTIFICATION_STATE = {
  Unread: 'Unread',
  Read: 'Read',
} as const satisfies Record<_NotificationState, _NotificationState>

export type NotificationState = ValueOf<typeof NOTIFICATION_STATE>

export type NotificationType = _NotificationType

export interface Notification {
  id: number
  state: NotificationState
  type: NotificationType
  metadata: {
    [key: string]: string
  }
  createdAt: Date
}
