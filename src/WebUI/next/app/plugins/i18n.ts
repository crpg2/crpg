// import { createI18n } from 'vue-i18n'

// // @ts-expect-error TODO:
// import en from '../../i18n/locales/en.yml'
// // @ts-expect-error TODO:
// import ru from '../../i18n/locales/ru.yml'

export default defineNuxtPlugin(({ vueApp }) => {
//   const i18n = createI18n({
//     legacy: false,
//     globalInjection: true,
//     missingWarn: false,
//     fallbackWarn: false,
//     // fallbackLocale: import.meta.env.VITE_LOCALE_FALLBACK,
//     fallbackLocale: 'en',
//     // locale: import.meta.env.VITE_LOCALE_DEFAULT,
//     locale: 'en',
//     messages: {
//       en,
//       ru,
//       // ru,
//     },
//     datetimeFormats: {
//       en: {
//         long: {
//           day: 'numeric',
//           hour: 'numeric',
//           minute: 'numeric',
//           month: 'short',
//           weekday: 'short',
//           year: 'numeric',
//         },
//         short: {
//           day: 'numeric',
//           hour: 'numeric',
//           hour12: false,
//           minute: 'numeric',
//           month: 'numeric',
//           year: 'numeric',
//         },
//         time: {
//           hour: 'numeric',
//           minute: 'numeric',
//         },
//       },
//       ru: {
//         long: {
//           day: 'numeric',
//           hour: 'numeric',
//           minute: 'numeric',
//           month: 'short',
//           weekday: 'short',
//           year: 'numeric',
//         },
//         short: {
//           dateStyle: 'short',
//         },
//         time: {
//           hour: 'numeric',
//           minute: 'numeric',
//         },
//       },
//       cn: {
//         long: {
//           day: 'numeric',
//           hour: 'numeric',
//           minute: 'numeric',
//           month: 'short',
//           weekday: 'short',
//           year: 'numeric',
//         },
//         short: {
//           dateStyle: 'short',
//         },
//         time: {
//           hour: 'numeric',
//           minute: 'numeric',
//         },
//       },
//     },
//     numberFormats: {
//       en: {
//         decimal: {
//           maximumFractionDigits: 3,
//           style: 'decimal',
//         },
//         percent: {
//           minimumFractionDigits: 2,
//           style: 'percent',
//         },
//         second: {
//           maximumFractionDigits: 3,
//           style: 'unit',
//           unit: 'second',
//           unitDisplay: 'narrow',
//         },
//       },
//       ru: {
//         decimal: {
//           maximumFractionDigits: 3,
//           style: 'decimal',
//         },
//         percent: {
//           minimumFractionDigits: 2,
//           style: 'percent',
//         },
//         second: {
//           maximumFractionDigits: 3,
//           style: 'unit',
//           unit: 'second',
//           unitDisplay: 'narrow',
//         },
//       },
//       cn: {
//         decimal: {
//           maximumFractionDigits: 3,
//           style: 'decimal',
//         },
//         percent: {
//           minimumFractionDigits: 2,
//           style: 'percent',
//         },
//         second: {
//           maximumFractionDigits: 3,
//           style: 'unit',
//           unit: 'second',
//           unitDisplay: 'narrow',
//         },
//       },
//     },
//     pluralRules: {
//       ru: (choice: number, choicesLength: number) => {
//         if (choice === 0) {
//           return 0
//         }

  //         const teen = choice > 10 && choice < 20
  //         const endsWithOne = choice % 10 === 1
  //         if (!teen && endsWithOne) {
  //           return 1
  //         }
  //         if (!teen && choice % 10 >= 2 && choice % 10 <= 4) {
  //           return 2
  //         }

  //         return choicesLength < 4 ? 2 : 3
  //       },
  //     },
  //   })

//   vueApp.use(i18n)
})
