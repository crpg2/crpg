import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

// https://nuxt.com/docs/4.x/getting-started/testing
export default defineConfig({
  test: {
    projects: [
      {
        test: {
          setupFiles: path.resolve(__dirname, './test/setup.ts'),
          name: 'unit',
          // include: ['test/unit/**/*.{spec,test}.ts'],
          environment: 'jsdom',
        },
        resolve: {
          alias: {
            '~': path.resolve(__dirname, './app'),
            '#api': path.resolve(__dirname, './generated/api'),
          },
        },
      },
    ],
  },
})
