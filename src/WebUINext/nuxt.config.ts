import type { Plugin } from 'vite'

import tailwindcss from '@tailwindcss/vite'
import json5 from 'json5'
import { fileURLToPath } from 'node:url'

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
// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/ui',
    '@nuxt/test-utils/module',
    '@nuxt/eslint',
    '@pinia/nuxt',
    '@vueuse/nuxt',
    '@hey-api/nuxt',
    '@nuxtjs/i18n',
    '@nuxt/image',
  ],
  ssr: false,
  devtools: { enabled: false },
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
    './assets/css/main.css',
  ],
  // https://ui.nuxt.com/getting-started/installation/nuxt#options
  ui: {
    // TODO:
    fonts: false,
    theme: {
      // colors: ['primary', 'secondary', 'neutral'],
    },
  },
  runtimeConfig: {
    public: {
      HH: import.meta.env.NUXT_PUBLIC_HH,
      api: {
        baseUrl: import.meta.env.NUXT_PUBLIC_API_BASE_URL,
      },
    },
  },
  alias: {
    '~root': fileURLToPath(new URL('../../', import.meta.url)),
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
  nitro: { compressPublicAssets: true },
  vite: {
    optimizeDeps: {
      include: [
        /**
         * in development mode we make a bundle of modules at once, so that we don't expect JIT bundling when switching between different pages where there are unbundled new modules.
         */
        '@vuelidate/core',
        '@vuelidate/validators',
        'tailwind-variants',
        '@tanstack/vue-table',
        '@number-flow/vue',
        '@internationalized/date',
      ],
    },
    plugins: [
      tailwindcss(),
      JSON5(),
    ],
    envPrefix: ['VITE_', 'NUXT_PUBLIC_'],
  },
  eslint: {
    config: {
      standalone: false,
      stylistic: true,
    },
  },
  heyApi: {
    autoImport: false,
    config: {
      input: 'https://localhost:8000/swagger/v1/swagger.json', // TODO: to env
      output: {
        path: './app/api',
        // clean: false,
        format: false,
        // format: 'prettier',
        lint: false,
        // lint: 'eslint',
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
          auth: false,
        },
        {
          name: '@hey-api/transformers',
          dates: true,
        },
      ],
      parser: {
        patch: {
          schemas: {
            CharacterStatisticsViewModel: (schema: any) => convertDateTimeToTimestamp(schema, 'playTime'),
            ClanViewModel: (schema: any) => convertDateTimeToTimestamp(schema, 'armoryTimeout'),
            UpdateClanCommand: (schema: any) => convertDateTimeToTimestamp(schema, 'armoryTimeout'),
            CreateClanCommand: (schema: any) => convertDateTimeToTimestamp(schema, 'armoryTimeout'),
            RestrictCommand: (schema: any) => convertDateTimeToTimestamp(schema, 'duration'),
            RestrictionPublicViewModel: (schema: any) => convertDateTimeToTimestamp(schema, 'duration'),
            RestrictionViewModel: (schema: any) => convertDateTimeToTimestamp(schema, 'duration'),
            ItemWeaponComponentViewModel: (schema: any) => {
              schema.properties.itemUsage.enum = [
                'long_bow',
                'bow',
                'crossbow',
                'crossbow_light',
                'polearm_couch',
                'polearm_bracing',
                'polearm_pike',
                'polearm',
              ]
            },
            ItemType: (schema: any) => {
              schema.enum.push(...['Ranged', 'Ammo'])
            },
            WeaponFlags: (schema: any) => {
              schema.enum.push(...['CanReloadOnHorseback', 'CantUseOnHorseback'])
            },
            ItemMountComponentViewModel: (schema: any) => {
              schema.properties.familyType.enum = [0, 1, 2, 3] // Undefined: 0, Horse: 1, Camel: 2, EBA: 3
              schema.properties.familyType.type = 'integer'
            },
            ItemArmorComponentViewModel: (schema: any) => {
              schema.properties.familyType.enum = [0, 1, 2, 3] // Undefined: 0, Horse: 1, Camel: 2, EBA: 3
              schema.properties.familyType.type = 'integer'
            },
          },
          // parameters: {
          //   // from: convertDateTimeToString,
          //   region: (parameter) => {
          //     // parameter.schema.type = 'integer'
          //   },
          // },
        },
      },
    },
  },
  i18n: {
    // debug: true,
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
    provider: 'server',
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

// convert date-time format to timestamp
function convertDateTimeToTimestamp(schema: any, key: string) {
  delete schema.properties[key].format
  schema.properties[key].type = 'number'
}

// convert date-time format to timestamp
function convertDateTimeToString(parameter: any) {
  console.log('parameter', parameter)
  delete parameter.schema.format
  parameter.schema.type = 'string'
}
