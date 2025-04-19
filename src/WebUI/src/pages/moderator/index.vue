<script setup lang="ts">
import { getRestrictions } from '~/services/restriction-service'

definePage({
  meta: {
    roles: ['Moderator', 'Admin'],
  },
})

const { state: restrictions } = useAsyncState(
  () => getRestrictions(),
  [],
)
</script>

<template>
  <div class="container">
    <RestrictionsTable :restrictions />
  </div>
</template>
