<script setup lang="ts">
import { ErrorResponse } from 'oidc-client-ts'

import { userManager } from '~/services/auth-service'

definePageMeta({
  layout: 'empty',
  skipAuth: true,
  middleware: [
    async () => {
      try {
        await userManager.signinCallback()
        return navigateTo({ name: 'characters' })
      }

      catch (error: unknown) {
        if (error instanceof ErrorResponse && error?.error === 'access_denied') {
          // TODO:
          // return { name: 'Banned' }
        }

        return navigateTo({ name: 'index' })
      }
    },
  ],
})
</script>

<template>
  <div />
</template>
