import { defineVitestConfig } from '@nuxt/test-utils/config'

export default defineVitestConfig({
  test: {
    clearMocks: true,
    // coverage: {
    //   exclude: ['node_modules/', './src/__test__/unit/index.ts', '**/*.spec.ts'],
    //   provider: 'v8',
    //   reporter: ['json', 'text', 'html'],
    // },
    environment: 'happy-dom',
    include: ['./app/**/*.spec.ts'],
    // setupFiles: ['./src/__test__/unit/setup.ts'],
  },
})
