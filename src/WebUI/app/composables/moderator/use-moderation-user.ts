import type { UserPrivate } from '~/models/user'

const moderationUserKey: InjectionKey<Ref<UserPrivate>> = Symbol('ModerationUser')

export const useModerationUserProvider = (user: Ref<UserPrivate>) => {
  provide(moderationUserKey, user)
}

export const useModerationUser = () => {
  const moderationUser = injectStrict(moderationUserKey)

  return {
    moderationUser,
  }
}
