// https://github.com/vueuse/vueuse/issues/1592#issuecomment-1381020982
import type { UseTimeAgoMessages, UseTimeAgoUnitNamesDefault } from '@vueuse/core'

import { useTimeAgo } from '@vueuse/core'

export const useLocaleTimeAgo = (date: Date) => {
  const { t } = useI18n()

  const I18N_MESSAGES: UseTimeAgoMessages<UseTimeAgoUnitNamesDefault> = {
    day: (n, past) =>
      n === 1
        ? past
          ? t('timeAgo.yesterday')
          : t('timeAgo.tomorrow')
        : `${n} ${t(`timeAgo.day`, n)}`,
    future: n => (n.match(/\d/) ? t('timeAgo.in', [n]) : n),
    hour: n => `${n} ${t('timeAgo.hour', n)}`,
    invalid: '',
    justNow: t('timeAgo.just-now'),
    minute: n => `${n} ${t('timeAgo.minute', n)}`,
    month: (n, past) =>
      n === 1
        ? past
          ? t('timeAgo.last-month')
          : t('timeAgo.next-month')
        : `${n} ${t(`timeAgo.month`, n)}`,
    past: n => (n.match(/\d/) ? t('timeAgo.ago', [n]) : n),
    second: n => `${n} ${t(`timeAgo.second`, n)}`,
    week: (n, past) =>
      n === 1
        ? past
          ? t('timeAgo.last-week')
          : t('timeAgo.next-week')
        : `${n} ${t(`timeAgo.week`, n)}`,
    year: (n, past) =>
      n === 1
        ? past
          ? t('timeAgo.last-year')
          : t('timeAgo.next-year')
        : `${n} ${t(`timeAgo.year`, n)}`,
  }

  return useTimeAgo(date, {
    fullDateFormatter: (date: Date) => date.toLocaleDateString(),
    messages: I18N_MESSAGES,
  })
}
