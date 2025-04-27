// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/test-utils/module',
    '@nuxt/eslint',
    '@nuxtjs/tailwindcss',
  ],
  ssr: false,
  devtools: { enabled: false },
  devServer: {
    host: '0.0.0.0',
    port: 8080,
  },
  future: {
    compatibilityVersion: 4,
  },
  experimental: { typedPages: true },
  compatibilityDate: '2025-03-21',
  eslint: {
    config: {
      stylistic: true,
    },
  },
})
