<script setup lang="ts">
import type { RouteLocationRaw } from 'vue-router'
import type { RouteNamedMap } from 'vue-router/auto-routes'

const { t } = useI18n()

definePageMeta({
  layoutOptions: {
    bg: 'background-3.webp',
  },
})

const helpHubNavigation = computed<{ name: keyof RouteNamedMap, label: string }[]>(() => [
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
])
</script>

<template>
  <UContainer
    class="
      space-y-12 py-8
      md:py-16
    "
  >
    <UiHeading :title="$t('help.title')" />

    <div class="flex justify-center gap-2">
      <NuxtLink
        v-for="{ name, label } in helpHubNavigation" :key="name"
        v-slot="{ isExactActive }"
        :to="({ name } as RouteLocationRaw)"
      >
        <UButton
          color="neutral"
          variant="link"
          active-variant="soft"
          active-color="primary"
          :active="isExactActive"
          size="xl"
          :label
        />
      </NuxtLink>
    </div>

    <div
      class="
        mx-auto
        md:max-w-2xl
      "
    >
      <NuxtPage />
    </div>
  </UContainer>
</template>
