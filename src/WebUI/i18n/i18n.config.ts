export default defineI18nConfig(() => {
  return {
    fallbackLocale: 'en',
    globalInjection: true,
    missingWarn: false,
    fallbackWarn: false,
    legacy: false,
    datetimeFormats: {
      en: {
        long: {
          day: 'numeric',
          hour: 'numeric',
          minute: 'numeric',
          month: 'short',
          weekday: 'short',
          year: 'numeric',
        },
        short: {
          day: 'numeric',
          hour: 'numeric',
          hour12: false,
          minute: 'numeric',
          month: 'numeric',
          year: 'numeric',
        },
        time: {
          hour: 'numeric',
          minute: 'numeric',
        },
      },
      ru: {
        long: {
          day: 'numeric',
          hour: 'numeric',
          minute: 'numeric',
          month: 'short',
          weekday: 'short',
          year: 'numeric',
        },
        short: {
          day: 'numeric',
          hour: 'numeric',
          hour12: false,
          minute: 'numeric',
          month: 'numeric',
          year: 'numeric',
        },
        time: {
          hour: 'numeric',
          minute: 'numeric',
        },
      },
    },
    numberFormats: {
      en: {
        decimal: {
          maximumFractionDigits: 3,
          style: 'decimal',
        },
        percent: {
          minimumFractionDigits: 2,
          style: 'percent',
        },
        second: {
          maximumFractionDigits: 3,
          style: 'unit',
          unit: 'second',
          unitDisplay: 'narrow',
        },
        compact: {
          notation: 'compact',
          compactDisplay: 'short',
          maximumFractionDigits: 1,
        },
      },
      ru: {
        decimal: {
          maximumFractionDigits: 3,
          style: 'decimal',
        },
        percent: {
          minimumFractionDigits: 2,
          style: 'percent',
        },
        second: {
          maximumFractionDigits: 3,
          style: 'unit',
          unit: 'second',
          unitDisplay: 'narrow',
        },
        compact: {
          notation: 'compact',
          compactDisplay: 'short',
          maximumFractionDigits: 1,
        },
      },
    },
    pluralRules: {
      ru: (choice: number, choicesLength: number) => {
        if (choice === 0) {
          return 0
        }

        const teen = choice > 10 && choice < 20
        const endsWithOne = choice % 10 === 1
        if (!teen && endsWithOne) {
          return 1
        }
        if (!teen && choice % 10 >= 2 && choice % 10 <= 4) {
          return 2
        }

        return choicesLength < 4 ? 2 : 3
      },
    },
  }
})
