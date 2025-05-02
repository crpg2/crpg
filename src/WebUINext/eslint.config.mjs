// @ts-check
import antfu from '@antfu/eslint-config'
import tailwind from 'eslint-plugin-tailwindcss'

import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt(
  tailwind.configs['flat/recommended'],
  antfu({
    lessOpinionated: true,
    vue: true,
    typescript: true,
    stylistic: true,
    rules: {
      'perfectionist/sort-imports': [
        'error',
        {
          groups: [
            'type',
            ['builtin', 'external'],
            'internal-type',
            'internal',
            ['parent-type', 'sibling-type', 'index-type'],
            ['parent', 'sibling', 'index'],
            'object',
            'unknown',
          ],
        },
      ],
    },
  }),
).overrideRules({
  '@typescript-eslint/no-unused-vars': ['warn'],
})
