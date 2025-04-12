import antfu from '@antfu/eslint-config'
import tailwind from 'eslint-plugin-tailwindcss'

export default antfu(
  {
    lessOpinionated: true,
    vue: true,
    typescript: true,
    stylistic: true,
  },
  {
    files: ['**/*.vue'],
    rules: {
      'vue/operator-linebreak': ['error', 'before'],
      'vue/max-attributes-per-line': ['warn', { singleline: { max: 3 }, multiline: { max: 2 } }],
    },
  },
  ...tailwind.configs['flat/recommended'],
  {
    rules: {
      'ts/no-use-before-define': 'off',
      'style/max-statements-per-line': ['error', { max: 2 }],
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
      'unused-imports/no-unused-vars': ['warn'],
    },
  },
)
