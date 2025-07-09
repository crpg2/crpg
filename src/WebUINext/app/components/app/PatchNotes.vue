<script setup lang="ts">
import type { PatchNote } from '~/models/patch-note'

import { useLocaleTimeAgo } from '~/composables/utils/use-locale-time-ago'

const { patchNotes } = defineProps<{ patchNotes: PatchNote[] }>()

const latestPatch = computed(() => patchNotes[0]!)

const timeAgo = useLocaleTimeAgo(latestPatch.value.createdAt)
</script>

<template>
  <div class="space-y-1">
    <a
      :href="latestPatch.url"
      target="_blank"
      class="
        group block rounded-full bg-elevated/50 px-5 py-4 shadow-xl backdrop-blur
        hover:shadow-none
      "
    >
      <UiDataCell>
        <template #leftContent>
          <UIcon name="crpg:trumpet" class="size-8 text-primary" />
        </template>
        <div
          class="
            max-w-72 truncate font-bold text-highlighted
            group-hover:text-default
          "
        >
          {{ latestPatch.title || $t('patchNotes.latestPatch') }}
        </div>
        <div class="text-2xs leading-none">{{ timeAgo }}</div>
        <template #rightContent>
          <UBadge :label="latestPatch.tagName" />
        </template>
      </UiDataCell>
    </a>
    <div
      v-if="patchNotes.length > 1"
      class="pl-5"
    >
      <ULink
        href="https://github.com/namidaka/crpg/releases"
        target="_blank"
        class="text-2xs"
      >
        {{ $t('patchNotes.showAllPatches', { count: patchNotes.length - 1 }) }}
      </ULink>
    </div>
  </div>
</template>
