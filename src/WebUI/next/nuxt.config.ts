// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/test-utils/module',
    '@nuxt/eslint',
    '@nuxtjs/tailwindcss',
    '@pinia/nuxt',
    '@vueuse/nuxt',
    '@hey-api/nuxt',
    '@nuxtjs/i18n',
  ],
  ssr: false,
  devtools: { enabled: false },
  app: {
    head: {

      title: 'cRPG: Multiplayer Mod for Mount & Blade Bannerlord',
      htmlAttrs: {
        lang: 'en',
      },
      bodyAttrs: {
        class: 'font-sans',
      },
      charset: 'utf-8',
      viewport: 'width=device-width, initial-scale=1',
      meta: [
        {
          name: 'description',
          content: 'cRPG is a mod for Mount & Blade II Bannerlord that adds persistence (experience, gold, stats, items) to the multiplayer.',
        },
        {
          name: 'theme-color',
          content: '#0f0f0e',
        },
      ],
      link: [
        {
          rel: 'apple-touch-icon',
          sizes: '180x180',
          href: '/apple-touch-icon.png',
        },
        {
          rel: 'icon',
          type: 'image/png',
          sizes: '32x32',
          href: '/favicon-32x32.png',
        },
        {
          rel: 'icon',
          type: 'image/png',
          sizes: '16x16',
          href: '/favicon-16x16.png',
        },
        {
          rel: '#0f0f0e',
          color: 'image/png',
          href: '//safari-pinned-tab.svg',
        },
        {
          rel: 'manifest',
          href: '/site.webmanifest',
        },
      ],
      script: [
        {
          type: 'application/ld+json',
          innerHTML: `
            {
                "@context": "https://schema.org",
                "@type": "Organization",
                "url": "https://c-rpg.eu",
                "logo": "https://c-rpg.eu/crpg.png"
            }`,
        },
      ],
    },
  },
  css: [
    './assets/themes/oruga-tailwind/index.css',
  ],
  devServer: {
    host: '0.0.0.0',
    port: 8080,
  },
  future: {
    compatibilityVersion: 4,
  },
  experimental: { typedPages: true },
  compatibilityDate: '2025-03-21',
  nitro: { compressPublicAssets: true },
  eslint: {
    config: {
      stylistic: true,
    },
  },
  heyApi: {
    config: {
      // input: 'https://localhost:8000/swagger/v1/swagger.json',
      input: './app/swagger.json',
      output: {
        format: 'prettier',
        lint: 'eslint',
        path: './app/api',
      },
      plugins: [
        {
          name: '@hey-api/client-nuxt',
          runtimeConfigPath: './app/api.config.ts',
        },
        {
          name: '@hey-api/typescript',
          enums: false,
        },
        {
          name: '@hey-api/sdk',
          transformer: '@hey-api/transformers',
          asClass: false,
          auth: false,
        },
        {
          name: '@hey-api/transformers',
          dates: true,
        },
      ],
    },
  },
  //   compositionOnly: true,
  //   include: [fileURLToPath(new URL('./locales/**', import.meta.url))],
  //   runtimeOnly: true,
  //   strictMessage: false,
  i18n: {
    bundle: {
      optimizeTranslationDirective: false,
    },
    detectBrowserLanguage: {
      useCookie: true,
      cookieKey: 'i18n_redirected',
      redirectOn: 'root', // recommended
    },
    lazy: true,
    // types: 'composition',
    strategy: 'no_prefix',
    defaultLocale: 'en',
    locales: [
      { code: 'en', name: 'English', file: 'en.yml' },
    ],
  },
})
