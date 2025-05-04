import type { UserPrivate } from '~/models/user'

export const moderationUserKey: InjectionKey<Ref<UserPrivate | null>> = Symbol('ModerationUser')
