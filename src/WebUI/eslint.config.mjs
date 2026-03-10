// @ts-check
import antfu from '@antfu/eslint-config'
import eslintPluginBetterTailwindcss from 'eslint-plugin-better-tailwindcss'
import { fileURLToPath } from 'node:url'
import eslintParserVue from 'vue-eslint-parser'

import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt(
  {
    ignores: ['generated/api'],
  },
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
            'type-import',
            ['value-builtin', 'value-external'],
            'type-internal',
            'value-internal',
            ['type-parent', 'type-sibling', 'type-index'],
            ['value-parent', 'value-sibling', 'value-index'],
            'ts-equals-import',
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
    extends: [
      eslintPluginBetterTailwindcss.configs.recommended,
    ],
    rules: {
      'better-tailwindcss/enforce-consistent-line-wrapping': ['warn', { printWidth: 100 }],
      'better-tailwindcss/no-unknown-classes': ['warn'],
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
