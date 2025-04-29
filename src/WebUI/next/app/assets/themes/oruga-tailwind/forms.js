const svgToDataUri = require('mini-svg-data-uri')
const plugin = require('tailwindcss/plugin')

// TODO: rewrite to ts
// ref: https://github.com/tailwindlabs/tailwindcss-forms/blob/master/src/index.js
module.exports = plugin(({ addBase, theme }) => {
  addBase({
    [`[type = 'checkbox'], [type='radio']`]: {
      'appearance': 'none',
      // height: theme('spacing.0-8'),
      // width: theme('spacing.0-8'),
      'display': 'inline-block',
      'userSelect': 'none',
      'verticalAlign': 'middle',
      // backgroundColor: theme('colors.white'),
      // borderWidth: theme('borderWidth.2'),
      // borderColor: theme('colors.black.100'),
      '&:checked': {
        backgroundPosition: 'center',
      },
      '&:focus': {
        boxShadow: 'none',
        outline: 'none',
      },
    },
    [`[type='checkbox']:checked`]: {
      backgroundImage: `url("${svgToDataUri(
        `<svg xmlns="http://www.w3.org/2000/svg" fill="${theme(
          'colors.content.600',
        )}" viewBox="0 0 10 8"><path fill-rule="evenodd" clip-rule="evenodd" d="M9.33 1.414 3.625 7.121 0 3.498l1.414-1.415 2.21 2.21L7.917 0 9.33 1.414Z"/></svg>`,
      )}")`,
      backgroundRepeat: 'no-repeat',
      backgroundSize: '75%',
    },
    [`
          [type='text'],
          [type='email'],
          [type='url'],
          [type='password'],
          [type='number'],
          [type='date'],
          [type='datetime-local'],
          [type='month'],
          [type='search'],
          [type='tel'],
          [type='time'],
          [type='week'],
          [multiple],
          textarea,
          select
        `]: {
      '-moz-appearance': 'textfield',
      '&::-webkit-inner-spin-button, &::-webkit-outer-spin-button': {
        '-webkit-appearance': 'none',
      },
      '&::placeholder': {
        // opacity: 0,
        // fontSize: theme('fontSize.sm'),
        // 'transition-delay': theme('transitionDelay.0'),
        // 'transition-duration': theme('transitionDuration.250'),
        // 'transition-property': theme('transitionProperty.opacity'),
        // 'transition-timing-function': theme(
        //     'transitionTimingFunction.linear'
        // )
      },
      '&:focus': {
        outline: 'none',
        // '&::placeholder': {
        //     opacity: 1,
        //     'transition-delay': theme('transitionDelay.150')
        // }
      },

      'appearance': 'none',

      'backgroundColor': 'transparent',
    },
    // [`[type='radio']`]: {
    //   borderRadius: theme('borderRadius.full'),
    //   '&:checked': {
    //     backgroundSize: '88%',
    //     backgroundImage: `url("${svgToDataUri(
    //       `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10" fill="${theme(
    //         'colors.primary.100'
    //       )}"><circle cx="5" cy="5" r="3"/></svg>`
    //     )}")`,
    //   },
    // },
  })
})
