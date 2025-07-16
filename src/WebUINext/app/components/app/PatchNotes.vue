<script setup lang="ts">
import type { PatchNote } from '~/models/patch-note'

import { useLocaleTimeAgo } from '~/composables/utils/use-locale-time-ago'

const { patchNotes } = defineProps<{ patchNotes: PatchNote[] }>()

const latestPatch = computed(() => patchNotes[0]!)

const timeAgo = useLocaleTimeAgo(latestPatch.value.createdAt)
</script>

<template>
  <div>
    <NuxtLink
      :href="latestPatch.url"
      target="_blank"
    >
      <UCard
        variant="soft"
        :ui="{
          root: 'bg-elevated rounded-full backdrop-blur group shadow-xl hover:shadow-none',
          body: 'sm:p-4',
        }"
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
          <div class="text-2xs leading-none">
            {{ timeAgo }}
          </div>
          <template #rightContent>
            <UBadge :label="latestPatch.tagName" />
          </template>
        </UiDataCell>
      </UCard>
    </NuxtLink>

    <div
      v-if="patchNotes.length > 1"
      class="mt-2 pl-5"
    >
      <ULink
        href="https://github.com/namidaka/crpg/releases"
        target="_blank"
        class="text-xs"
      >
        {{ $t('patchNotes.showAllPatches', { count: patchNotes.length - 1 }) }}
      </ULink>
    </div>
  </div>
</template>
