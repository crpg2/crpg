import type { Config } from 'tailwindcss'

import typography from '@tailwindcss/typography'
import defaultTheme from 'tailwindcss/defaultTheme'

// import forms from './src/assets/themes/oruga-tailwind/forms'

export default {
  content: ['index.html', './src/**/*.{js,jsx,ts,tsx,vue,html}'],
  plugins: [
    typography,
    // forms
  ],
  theme: {
    container: {
      center: true,
      padding: {
        DEFAULT: '1rem',
      },
    },

    // TODO:
    screens: {
      'sm': '640px',
      'md': '768px',
      'xl': '1280px',
      'lg': '1024px',
      '2xl': '1536px',
    },

    // TODO:
    colors: {
      base: {
        100: '#0F0F0E',
        200: '#171716',
        300: '#272723',
        400: '#3C3B36',
        500: '#555552',
        600: '#FFFFFF',
      },
      bg: {
        main: '#0F0F0E',
      },
      black: '#000000',

      border: {
        200: '#232527',
        300: '#404040',
      },
      content: {
        100: '#FFFFFF',
        200: '#B6B6B6',
        300: '#8F8F8F',
        400: '#626262',
        500: '#3F3F3F',
        600: '#0D0D0D',

        link: {
          DEFAULT: '#D2BB8A',
          hover: '#AE9257',
        },
      },

      current: 'currentColor',

      inherit: 'inherit',

      more: {
        support: '#C99E34',
      },

      primary: {
        DEFAULT: '#D2BB8A',
        hover: '#AE9257',
        // disabled =  DEFAULT + 30% opacity
      },

      status: {
        danger: '#BF3838',
        success: '#53825A',
        warning: '#C29F47',
      },

      transparent: 'transparent',

      white: '#ffffff',
    },

    extend: {
      borderRadius: {
        sm: '0.188rem', // 2px
      },

      fontSize: {
        '2xl': ['36px', '42px'],
        '2xs': ['12px', '18px'],
        '3xl': ['42px', '48px'],
        '3xs': ['10px', '14px'],
        'lg': ['24px', '28px'],
        'sm': ['18px', '23px'],
        'title-hero': ['42px', '48px'],
        'title-lg': ['21px', '27px'],
        'title-md': ['15px', '21px'],
        'xl': ['32px', '38px'],
        'xs': ['15px', '21px'],
      },

      minHeight: {
        // screen: 'calc(100vh + 1px)',
        screen: '100vh',
      },

      minWidth: {
        36: '9rem', /* 144px */
        48: '12rem', /* 192px */
        60: '15rem', /* 240px */
        9: '2.25rem', /* 36px */
        initial: 'initial',
      },

      opacity: {
        15: '0.15',
      },

      spacing: {
        10.5: '2.625rem', /* 42px */
        13.5: '3.375rem', /* 54px */
        4.5: '1.125rem', /* 18px */
      },

      zIndex: {
        100: '100',
      },

      // https://tailwindcss.com/docs/animation#using-custom-values
      animation: {
        ping: 'ping 1s linear infinite',
      },

      keyframes: {
        ping: {
          '75%, 100%': { opacity: '0', transform: 'scale(1.5)' },
        },
      },

      textUnderlineOffset: {
        6: '6px',
      },

      // ref: https://github.com/tailwindlabs/tailwindcss-typography/blob/master/src/styles.js
      typography: ({ theme }) => {
        // console.log(theme('spacing'));
        return {
          DEFAULT: {
            css: {
              '--tw-prose-invert-counters': theme('colors.content.200'),
              'a': {
                '&:hover': {
                  color: theme('colors.content.link.hover'),
                },
                'color': theme('colors.content.link.DEFAULT'),
              },
              'fontSize': theme('fontSize.xs[0]'),
              'h3': {
                fontSize: theme('fontSize.lg[0]'),
                marginBottom: theme('spacing')['2.5'],
                marginTop: theme('spacing')['2.5'],
              },
              'h4': {
                fontSize: theme('fontSize.lg[0]'),
                marginBottom: theme('spacing')['2.5'],
                marginTop: theme('spacing')['2.5'],
              },
              'h5': {
                color: 'var(--tw-prose-headings)',
                fontSize: theme('fontSize.sm[0]'),
                fontWeight: theme('fontWeight.semibold'),
                marginBottom: theme('spacing')['2.5'],
                marginTop: theme('spacing')['2.5'],
              },
              'li': {
                marginBottom: theme('spacing')['2.5'],
                marginTop: theme('spacing')['0'],
              },
              'lineHeight': theme('fontSize.sm[1]'),
            },
          },

          invert: {
            css: {
              '--tw-prose-body': theme('colors.content.200'),
              '--tw-prose-bold': theme('colors.content.100'),
              '--tw-prose-headings': theme('colors.content.100'),
              // TODO:!
              '--tw-prose-bullets': 'var(--tw-prose-invert-bullets)',
              '--tw-prose-captions': 'var(--tw-prose-invert-captions)',
              '--tw-prose-code': 'var(--tw-prose-invert-code)',
              '--tw-prose-counters': 'var(--tw-prose-invert-counters)',
              '--tw-prose-hr': 'var(--tw-prose-invert-hr)',
              '--tw-prose-lead': 'var(--tw-prose-invert-lead)',
              '--tw-prose-links': 'var(--tw-prose-invert-links)',
              '--tw-prose-pre-bg': 'var(--tw-prose-invert-pre-bg)',
              '--tw-prose-pre-code': 'var(--tw-prose-invert-pre-code)',
              '--tw-prose-quote-borders': 'var(--tw-prose-invert-quote-borders)',
              '--tw-prose-quotes': 'var(--tw-prose-invert-quotes)',
              '--tw-prose-td-borders': 'var(--tw-prose-invert-td-borders)',
              '--tw-prose-th-borders': 'var(--tw-prose-invert-th-borders)',
            },
          },
        }
      },
    },
    fontFamily: {
      sans: ['Merriweather', ...defaultTheme.fontFamily.sans],
    },
  },
} satisfies Config
