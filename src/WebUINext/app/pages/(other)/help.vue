<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'
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
  <UContainer class="space-y-12 py-8 md:py-16">
    <UiHeading :title="$t('help.title')" />

    <div class="flex items-center justify-center gap-2">
      <NuxtLink
        v-for="{ name, label } in helpHubNavigation" :key="name"
        v-slot="{ isExactActive }"
        :to="({ name } as RouteLocationRaw)"
      >
        <UButton
          color="primary"
          :variant="isExactActive ? 'subtle' : 'outline'"
          size="lg"
          :label
        />
      </NuxtLink>
    </div>

    <div class="mx-auto max-w-4xl">
      <NuxtPage />
    </div>
  </UContainer>
</template>
