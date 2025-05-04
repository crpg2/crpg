const plugin = require('tailwindcss/plugin')

// TODO: rewrite to ts
// ref: https://github.com/tailwindlabs/tailwindcss-forms/blob/master/src/index.js
module.exports = plugin(({ addBase }) => {
  addBase({
    [`[type = 'checkbox'], [type='radio']`]: {
      'appearance': 'none',
      'display': 'inline-block',
      'userSelect': 'none',
      'verticalAlign': 'middle',
      '&:checked': {
        backgroundPosition: 'center',
      },
      '&:focus': {
        boxShadow: 'none',
        outline: 'none',
      },
    },
    [`[type='checkbox']:checked`]: {
      backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='%230D0D0D' viewBox='0 0 10 8'%3E%3Cpath fill-rule='evenodd' clip-rule='evenodd' d='M9.33 1.414 3.625 7.121 0 3.498l1.414-1.415 2.21 2.21L7.917 0 9.33 1.414Z'/%3E%3C/svg%3E");`,
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
      '&:focus': {
        outline: 'none',
      },
      'appearance': 'none',
      'backgroundColor': 'transparent',
    },
  })
})
