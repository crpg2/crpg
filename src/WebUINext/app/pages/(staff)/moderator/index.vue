<script setup lang="ts">
import { getRestrictions } from '~/services/restriction-service'

const {
  state: restrictions,
} = useAsyncState(
  () => getRestrictions(),
  [],
)
</script>

<template>
  <div class="container">
    <ModeratorRestrictionsTable :restrictions />
  </div>
</template>
