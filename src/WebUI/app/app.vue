<script setup lang="ts">
import type { Locale, Messages } from '@nuxt/ui'

import { en, ru } from '@nuxt/ui/locale'

import { usePageLoadingProvider } from '~/composables/app/use-page-loading'

const [activePageLoading] = usePageLoadingProvider()

const { locale } = useI18n()

const uiLocales: Record<string, Locale<Messages>> = {
  en,
  ru,
}
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
    :locale="uiLocales[locale] ?? en"
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
