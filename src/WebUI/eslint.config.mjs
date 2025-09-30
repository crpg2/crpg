// @ts-check
import antfu from '@antfu/eslint-config'
import eslintPluginBetterTailwindcss from 'eslint-plugin-better-tailwindcss'
import { fileURLToPath } from 'node:url'
import eslintParserVue from 'vue-eslint-parser'

import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt(
  { ignores: ['./app/api'] },
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
  {
    files: ['**/*.vue'],
    languageOptions: {
      parser: eslintParserVue,
    },
  },
  {
    plugins: {
      'better-tailwindcss': eslintPluginBetterTailwindcss,
    },
    rules: {
      // @ts-ignore
      ...eslintPluginBetterTailwindcss.configs['recommended-warn'].rules,
      // @ts-ignore
      ...eslintPluginBetterTailwindcss.configs['recommended-error'].rules,
      'better-tailwindcss/multiline': ['warn', { printWidth: 100 }],
      'better-tailwindcss/no-unregistered-classes': ['warn'],
      'better-tailwindcss/enforce-consistent-line-wrapping': ['off'],
    },
  },
  {
    settings: {
      'better-tailwindcss': {
        entryPoint: fileURLToPath(new URL('./app/assets/css/main.css', import.meta.url)),
      },
    },

  },
).overrideRules({
  '@typescript-eslint/no-unused-vars': ['warn'],
  'unused-imports/no-unused-vars': ['warn'],
})
