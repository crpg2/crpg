<script setup lang="ts">
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
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
    <AppBundledSprite />
  </UApp>
</template>
