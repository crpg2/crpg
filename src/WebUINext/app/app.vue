<script setup lang="ts">
import { usePageLoadingProvider } from '~/composables/app/use-page-loading'

const [activePageLoading] = usePageLoadingProvider()

const { start, finish } = useLoadingIndicator()
const { $poll } = useNuxtApp()

watch(activePageLoading, () => {
  activePageLoading.value ? start() : finish()
})
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
    {{ $poll.keys }}
    {{ activePageLoading }}

    <NuxtLoadingIndicator color="rgb(210 187 138)" />

    <NuxtRouteAnnouncer />

    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>

    <!-- TODO: FIXME: -->
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

    <!-- <Teleport to="body">
      <UiLoading :active="activePageLoading" full-page />
    </Teleport> -->
  </UApp>
</template>
