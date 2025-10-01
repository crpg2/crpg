import path from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig } from 'vitest/config'

const __dirname = path.dirname(fileURLToPath(import.meta.url))

// https://nuxt.com/docs/4.x/getting-started/testing
export default defineConfig({
  test: {
    projects: [
      {
        resolve: {
          alias: {
            '~': path.resolve(__dirname, './app'),
          },
        },
        test: {
          name: 'unit',
          include: ['test/unit/**/*.{spec,test}.ts'],
          environment: 'node',
        },
      },
    ],
  },
})
