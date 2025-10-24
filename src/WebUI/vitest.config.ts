import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'

const __dirname = dirname(fileURLToPath(import.meta.url))

// https://nuxt.com/docs/4.x/getting-started/testing
export default defineConfig({
  test: {
    projects: [
      {
        test: {
          name: 'unit',
          environment: 'node',
          clearMocks: true,
          setupFiles: resolve(__dirname, './test/setup.ts'),
        },
        resolve: {
          alias: {
            '~': resolve(__dirname, './app'),
            '#api': resolve(__dirname, './generated/api'),
          },
        },
      },
    ],
  },
})
