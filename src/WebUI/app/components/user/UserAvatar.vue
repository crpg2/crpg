<script setup lang="ts">
import type { AvatarProps } from '@nuxt/ui'

defineProps<{
  avatar: string
  name: string
  size: AvatarProps['size']
  isSelf?: boolean
}>()
</script>

<template>
  <div
    class="relative inline-flex"
  >
    <div
      v-if="isSelf"
      class="
        squircle absolute top-1/2 left-1/2 h-[117%] w-[117%] -translate-x-1/2 -translate-y-1/2
        bg-notification
      "
    />

    <UAvatar
      :src="avatar"
      :size
      :alt="name"
      class="squircle rounded-none"
    />
  </div>
</template>

<style>
.squircle {
  mask-image: url("data:image/svg+xml,%3csvg width='200' height='200' xmlns='http://www.w3.org/2000/svg'%3e%3cpath d='M100 0C20 0 0 20 0 100s20 100 100 100 100-20 100-100S180 0 100 0Z'/%3e%3c/svg%3e");
  mask-size: contain;
  mask-position: center;
  mask-repeat: no-repeat;
}
</style>
