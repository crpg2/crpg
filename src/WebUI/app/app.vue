<script setup lang="ts">
import { usePageLoadingProvider } from '~/composables/app/use-page-loading'

const [activePageLoading] = usePageLoadingProvider()
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

    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>

    <AppBundledSprite />

    <Teleport to="body">
      <UiLoading :active="activePageLoading" full-page />
    </Teleport>
  </UApp>
</template>
