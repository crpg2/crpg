import type { Plugin } from 'vite'

import tailwindcss from '@tailwindcss/vite'
import json5 from 'json5'
import { fileURLToPath } from 'node:url'

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/ui',
    '@nuxt/eslint',
    '@vueuse/nuxt',
    '@nuxtjs/i18n',
  ],
  ssr: false,
  devtools: { enabled: true },
  app: {
    head: {
      title: 'cRPG: Multiplayer Mod for Mount & Blade Bannerlord',
      htmlAttrs: {
        lang: 'en',
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
          href: '/safari-pinned-tab.svg',
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
    './assets/css/main.css',
  ],
  // https://ui.nuxt.com/getting-started/installation/nuxt#options
  ui: {
    fonts: false,
  },
  runtimeConfig: {
    public: {
      api: {
        baseUrl: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
      },
    },
  },
  alias: {
    '~root': fileURLToPath(new URL('../../', import.meta.url)),
    '#api': fileURLToPath(new URL('generated/api', import.meta.url)),
  },
  build: {
    transpile: [/vue-i18n/],
  },
  devServer: {
    host: '0.0.0.0',
    port: 8080,
  },
  experimental: {
    typedPages: true,
    defaults: {
      nuxtLink: {
        prefetch: false,
      },
    },
  },
  compatibilityDate: '2025-07-15',
  nitro: {
    compressPublicAssets: {
      gzip: true,
      brotli: false,
    },
    output: {
      dir: import.meta.env.OUT_DIR || '.output',
    },
  },
  vite: {
    plugins: [
      tailwindcss(),
      JSON5(),
    ],
    envPrefix: ['NUXT_PUBLIC_'],
  },
  eslint: {
    config: {
      standalone: false,
      stylistic: true,
    },
  },
  i18n: {
    compilation: {
      strictMessage: false,
    },
    bundle: {
      runtimeOnly: true,
      dropMessageCompiler: true,
    },
    strategy: 'no_prefix',
    defaultLocale: 'en',
    locales: [
      { code: 'en', file: 'en.yml' },
      { code: 'ru', file: 'ru.yml' },
    ],
  },
  icon: {
    mode: 'svg',
    class: 'fill-current',
    // provider: 'none', // TODO:
    customCollections: [
      {
        prefix: 'crpg',
        dir: './app/assets/icons',
      },
    ],
    clientBundle: {
      scan: true,
      includeCustomCollections: true,
      sizeLimitKb: 0,
    },
  },
})

function JSON5(): Plugin {
  const fileRegex = /\.json$/

  return {
    enforce: 'pre', // before vite-json
    name: 'vite-plugin-json5',
    transform(src, id) {
      if (fileRegex.test(id)) {
        let value

        try {
          value = json5.parse(src)
        }
        catch (error) {
          console.error(error)
        }

        return {
          code: value ? JSON.stringify(value) : src,
          map: null,
        }
      }
    },
  }
}
