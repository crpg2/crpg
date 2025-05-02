<script setup lang="ts">
import { disableBodyScroll, enableBodyScroll } from 'body-scroll-lock'

import { useModalCounter } from '~/composables/use-modal-count'

defineProps<{ closable?: boolean }>()

const emit = defineEmits<{
  hide: []
}>()

const { counter, decrease, increase } = useModalCounter()

const onShow = () => {
  if (counter.value === 0) {
    disableBodyScroll(document.querySelector('body')!, { reserveScrollBarGap: true })
  }

  increase()
}

const onHide = () => {
  decrease()

  if (counter.value === 0) {
    enableBodyScroll(document.querySelector('body')!)
  }

  emit('hide')
}

onBeforeUnmount(() => {
  onHide()
})
</script>

<template>
  <VDropdown
    positioning-disabled
    @apply-show="onShow"
    @apply-hide="onHide"
  >
    <template #default="popper">
      <slot v-bind="popper" />
    </template>

    <template #popper="popper">
      <OButton
        v-if="closable"
        class="!absolute -right-4 -top-4 z-10 shadow"
        icon-right="close"
        rounded
        size="sm"
        variant="secondary"
        @click="popper.hide"
      />
      <slot
        name="popper"
        v-bind="popper"
      />
    </template>
  </VDropdown>
</template>
