// @ts-check
import tailwind from 'eslint-plugin-tailwindcss'
import withNuxt from './.nuxt/eslint.config.mjs'

export default withNuxt(
  tailwind.configs['flat/recommended'],
).overrideRules({
  '@typescript-eslint/no-unused-vars': ['warn'],
})
