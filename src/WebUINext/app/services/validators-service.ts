import type { ErrorObject } from '@vuelidate/core'
import type { withI18nMessage as _withI18nMessage } from '@vuelidate/validators'

import * as validators from '@vuelidate/validators'
import { clanBannerKeyRegex, clanTagRegex } from '~root/data/constants.json'

export const errorMessagesToString = (errors: ErrorObject[]) => errors.map(e => e.$message).filter(Boolean).join(', ')

const withI18nMessage: typeof _withI18nMessage = (validator, options) => {
  const { t } = useI18n()
  const wrapper = validators.createI18nMessage({ t })
  return wrapper(validator, options)
}

export const required = () => withI18nMessage(validators.required)

export const sameAs = <E = string>(name: E | Ref<E>, compareName: string) => withI18nMessage(validators.sameAs(name, compareName), { withArguments: true })

export const minLength = (...params: Parameters<typeof validators.minLength>) => withI18nMessage(validators.minLength(...params), { withArguments: true })

export const maxLength = (...params: Parameters<typeof validators.maxLength>) => withI18nMessage(validators.maxLength(...params), { withArguments: true })

export const integer = () => withI18nMessage(validators.integer)

export const minValue = (...params: Parameters<typeof validators.minValue>) => withI18nMessage(validators.minValue(...params), { withArguments: true })

export const maxValue = (...params: Parameters<typeof validators.maxValue>) => withI18nMessage(validators.maxValue(...params), { withArguments: true })

export const clanTagPattern = () => withI18nMessage(validators.helpers.regex(new RegExp(clanTagRegex)))

export const clanBannerKeyPattern = () => withI18nMessage(
  validators.helpers.regex(new RegExp(clanBannerKeyRegex)),
)

export const discordLinkPattern = () => withI18nMessage(
  validators.helpers.regex(
    /(https?:\/\/)?(www.)?(discord.(gg|io|me|li)|discordapp.com\/invite)\/.+[a-z]/, // https://www.regextester.com/99527
  ),
)
