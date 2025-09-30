<script setup lang="ts">
import { getRestrictions } from '~/services/restriction-service'

const {
  state: restrictions,
  isLoading: loadingRestrictions,
} = useAsyncState(() => getRestrictions(), [])
</script>

<template>
  <UContainer>
    <ModeratorRestrictionsTable :restrictions :loading="loadingRestrictions" />
  </UContainer>
</template>
