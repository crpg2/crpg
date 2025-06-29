<script setup lang="ts">
import { usePageLoading } from '~/composables/utils/use-page-loading'

const { activePageLoading } = usePageLoading()
</script>

<template>
  <UApp
    :toaster="{
      position: 'top-center',
      duration: 1000,
    }"
    :tooltip="{
      delayDuration: 300,
    }"
  >
    <NuxtLoadingIndicator color="rgb(210 187 138)" />

    <NuxtRouteAnnouncer />

    <UiLoading :active="activePageLoading" full-page />

    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>

    <!-- <NuxtErrorBoundary>
      <NuxtLayout>
        <NuxtPage />
      </NuxtLayout>

      <template #error="{ error, clearError }">
        <UModal
          open
          :title="$t('error.title')"
        >
          <template #body>
            <p>An error occurred: {{ error }}</p>
            <button @click="clearError">
              Clear error
            </button>
          </template>
        </UModal>
      </template>
    </NuxtErrorBoundary> -->

    <AppBundledSprite />
  </UApp>
</template>
