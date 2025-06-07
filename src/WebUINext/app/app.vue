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
    <NuxtLoadingIndicator
      color="rgb(210 187 138)"
    />
    <NuxtRouteAnnouncer />
    <UiLoading :active="activePageLoading" full-page />
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
    <AppBundledSprite />
  </UApp>
</template>
