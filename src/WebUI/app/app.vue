<script setup lang="ts">
import type { Locale, Messages } from '@nuxt/ui'

import { en, ru } from '@nuxt/ui/locale'
import { client } from '#api/client.gen'

import { usePageLoading } from '~/composables/app/use-page-loading'

const [activePageLoading] = usePageLoading()

const { locale } = useI18n()

const uiLocales: Record<string, Locale<Messages>> = {
  en,
  ru,
}

function mapLocaleToApiLanguage(locale: string): string {
  return locale === 'cn' ? 'zh-CN' : locale
}

// console.log('dd', client.getConfig())

client.setConfig({
  // ...client.getConfig(),
  onRequest: [
    async ({ options }) => {
      options.headers.set('Accept-Language', mapLocaleToApiLanguage(locale.value))
    },
  ],
})
</script>

<template>
  <UApp
    :toaster="{
      position: 'top-center',
      duration: 2500,
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
