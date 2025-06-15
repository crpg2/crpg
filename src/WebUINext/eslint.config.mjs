// @ts-check
import antfu from '@antfu/eslint-config'
// @ts-expect-error TODO:
import tailwind from '@hyoban/eslint-plugin-tailwindcss'
import { fileURLToPath } from 'node:url'

import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt(
  antfu({
    lessOpinionated: true,
    vue: true,
    typescript: true,
    stylistic: true,
    settings:{
        "better-tailwindcss": {
            "entryPoint": "app/assets/css/main.css",
        }
    },
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
  ...tailwind.configs['flat/recommended'],
  {
    settings: {
      tailwindcss: {
        config: fileURLToPath(new URL('./app/assets/css/main.css', import.meta.url)),
      },
    },
  },
).overrideRules({
  '@typescript-eslint/no-unused-vars': ['warn'],
  'unused-imports/no-unused-vars': ['warn'],
})
