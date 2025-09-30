<script setup lang="ts">
import type { PatchNote } from '~/models/patch-note'

import { useLocaleTimeAgo } from '~/composables/utils/use-locale-time-ago'

const { patchNotes } = defineProps<{ patchNotes: PatchNote[] }>()

const latestPatch = computed(() => patchNotes.at(0))

const timeAgo = useLocaleTimeAgo(latestPatch.value?.createdAt || new Date())
</script>

<template>
  <div>
    <NuxtLink
      v-if="latestPatch"
      :href="latestPatch.url"
      target="_blank"
    >
      <UCard
        variant="soft"
        :ui="{
          root: 'bg-muted  backdrop-blur group shadow-xl hover:shadow-none',
          body: 'sm:p-2.5',
        }"
      >
        <UiDataCell>
          <template #leftContent>
            <UIcon name="crpg:trumpet" class="size-10 text-primary" />
          </template>

          <UiDataContent
            :label="latestPatch.title || $t('patchNotes.latestPatch')"
            :caption="timeAgo"
            ellipsis
            class="max-w-72"
          />

          <template #rightContent>
            <UBadge :label="latestPatch.tagName" variant="solid" />
          </template>
        </UiDataCell>
      </UCard>
    </NuxtLink>

    <UiTextView
      v-if="patchNotes.length > 1"
      class="mt-2 pl-5"
      variant="caption-sm"
    >
      <ULink
        href="https://github.com/namidaka/crpg/releases"
        target="_blank"
      >
        {{ $t('patchNotes.showAllPatches', { count: patchNotes.length - 1 }) }}
      </ULink>
    </UiTextView>
  </div>
</template>
