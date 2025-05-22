<script setup lang="ts">
import type { RouteNamedMap } from 'vue-router/auto-routes'

const { t } = useI18n()

definePageMeta({
  layoutOptions: {
    bg: 'background-3.webp',
  },
})

const helpHubNavigation: { name: keyof RouteNamedMap, label: string }[] = [
  {
    label: t('help.overview.navTitle'),
    name: 'help',
  },
  {
    label: t('help.buildSupport.navTitle'),
    name: 'help-build-support',
  },
  {
    label: t('help.joinClan.navTitle'),
    name: 'help-join-clan',
  },
  {
    label: t('help.FAQ.navTitle'),
    name: 'help-faq',
  },
]
</script>

<template>
  <div class="container py-8 md:py-16">
    <UiHeading :title="$t('help.title')" />

    <div class="mb-12 flex items-center justify-center gap-2">
      <NuxtLink
        v-for="{ name, label } in helpHubNavigation"
        :key="name"
        v-slot="{ isExactActive }"
        :to="{ name }"
      >
        <OButton
          :variant="isExactActive ? 'transparent-active' : 'secondary'"
          size="lg"
          :label
        />
      </NuxtLink>
    </div>

    <div class="mx-auto max-w-4xl">
      <NuxtPage />
    </div>
  </div>
</template>
