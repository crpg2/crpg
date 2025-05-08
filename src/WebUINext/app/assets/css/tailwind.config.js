import forms from './forms'

/** @type {import('tailwindcss').Config} */
export default {
  plugins: [forms],
  theme: {
    extend: {
      typography: ({ theme }) => {
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
  },
}
