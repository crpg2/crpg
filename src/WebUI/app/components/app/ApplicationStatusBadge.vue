<script setup lang="ts">
import type { BadgeProps } from '@nuxt/ui'

type ApplicationStatus = 'Accepted' | 'Declined' | 'Pending'

defineProps<{
  status: ApplicationStatus
  icon?: BadgeProps['icon']
  trailingIcon?: BadgeProps['icon']
}>()

const colorByApplicationStatus: Record<ApplicationStatus, BadgeProps['color']> = {
  Pending: 'info',
  Accepted: 'success',
  Declined: 'error',
}
</script>

<template>
  <UBadge
    variant="subtle"
    class="cursor-pointer"
    :icon
    :trailing-icon
    :color="colorByApplicationStatus[status]"
    :label="$t(`application.status.${status}`)"
  />
</template>
