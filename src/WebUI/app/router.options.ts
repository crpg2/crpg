import type { RouterConfig } from '@nuxt/schema'
import type { LocationQuery } from 'vue-router'

import { parse, stringify } from 'qs'

import { tryGetNumber } from '~/utils/number'

// ref: https://github.com/ljharb/qs/blob/main/lib/utils.js#L111
// TODO: spec
const decoder = (str: string): string | number | boolean | null | undefined => {
  const [isNumber, candidateToNumber] = tryGetNumber(str)

  if (isNumber) {
    return candidateToNumber
  }

  const keywords: Record<string, any> = {
    false: false,
    null: null,
    true: true,
    undefined,
  }

  if (str in keywords) {
    return keywords[str]
  }

  const strWithoutPlus = str.replace(/\+/g, ' ')

  try {
    return decodeURIComponent(strWithoutPlus)
  }
  // eslint-disable-next-line unused-imports/no-unused-vars
  catch (_e) {
    return strWithoutPlus
  }
}

const parseQuery = (query: string) =>
  parse(query, {
    decoder,
    ignoreQueryPrefix: true,
    strictNullHandling: true,
  }) as LocationQuery

const stringifyQuery = (query: Record<string, any>) =>
  stringify(query, {
    arrayFormat: 'brackets',
    encode: true,
    skipNulls: true,
    strictNullHandling: true,
  })

export default {
  parseQuery,
  // @ts-expect-error TODO:
  stringifyQuery,
} satisfies RouterConfig
