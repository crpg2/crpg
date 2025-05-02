import type { ErrorObject } from '@vuelidate/core'

import * as validators from '@vuelidate/validators'
import { clanBannerKeyRegex, clanTagRegex } from '~root/data/constants.json'

const withI18nMessage = () => {
  const { t } = useI18n()
  return validators.createI18nMessage({ t })
}

export const required = () => withI18nMessage()(validators.required)

// export const url = withI18nMessage()(validators.url)

export const sameAs = () => withI18nMessage()(validators.sameAs, { withArguments: true })

export const minLength = () => withI18nMessage()(validators.minLength, { withArguments: true })

export const maxLength = () => withI18nMessage()(validators.maxLength, { withArguments: true })

// export const integer = withI18nMessage()(validators.integer, { withArguments: true })

// export const minValue = withI18nMessage()(validators.minValue, { withArguments: true })

// export const maxValue = withI18nMessage()(validators.maxValue, { withArguments: true })

// export const clanTagPattern = withI18nMessage()(validators.helpers.regex(new RegExp(clanTagRegex)))

// export const clanBannerKeyPattern = withI18nMessage()(
//   validators.helpers.regex(new RegExp(clanBannerKeyRegex)),
// )

export const errorMessagesToString = (errors: ErrorObject[]) => errors.map(e => e.$message).join(', ')

// export const discordLinkPattern = withI18nMessage()(
//   validators.helpers.regex(
//     /(https?:\/\/)?(www.)?(discord.(gg|io|me|li)|discordapp.com\/invite)\/.+[a-z]/, // https://www.regextester.com/99527
//   ),
// )
