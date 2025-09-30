<script setup lang="ts">
import { LazyUserRestrictionModal } from '#components'

import type { UserRestrictionPublic } from '~/models/user'

const props = defineProps<{
  restriction: UserRestrictionPublic
}>()

const { t } = useI18n()

const joinRestrictionRemainingDuration = computed(() =>
  parseTimestamp(computeLeftMs(props.restriction.createdAt, Number(props.restriction.duration))),
)

const overlay = useOverlay()
const modal = overlay.create(LazyUserRestrictionModal, {
  props: {
    restriction: props.restriction,
  },
})
</script>

<template>
  <UBanner
    :ui="{
      center: '',
      title: 'text-highlighted text-lg',
    }"
    color="error"
    :title="$t('user.restriction.notification', { duration: $t('dateTimeFormat.dd:hh:mm', { ...joinRestrictionRemainingDuration }) })"
    :actions="[{
      color: 'neutral',
      size: 'sm',
      label: t('action.readMore'),
      onClick: () => {
        modal.open()
      },
    }]"
  />
</template>
