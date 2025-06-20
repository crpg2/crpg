<script setup lang="ts">
import type { HHEvent as HHEventType } from '~/services/hh-service'

import { useUserStore } from '~/stores/user'

// eslint-disable-next-line vue/prop-name-casing
defineProps<{ HHEvent: HHEventType }>()

const userStore = useUserStore()

const scrollToTop = () =>
  window.scrollTo({
    behavior: 'smooth',
    top: 0,
  })
</script>

<template>
  <footer class="relative mt-auto flex flex-wrap items-center justify-between border-t border-solid border-border-200 px-6 py-5 text-2xs text-content-300">
    <AppSocials
      patreon-expanded
      size="sm"
    />

    <div class="flex items-center gap-5">
      <AppHHTooltip
        v-if="userStore.user"
        :region="userStore.user!.region"
      >
        <UiDataCell class="hover:text-content-100">
          <template #leftContent>
            <UIcon
              name="crpg:gift"
              class="size-6 text-content-100"
            />
          </template>
          {{ $t('hh.tooltip-trigger', { region: $t(`region.${userStore.user!.region}`, 1) }) }}
          <template #rightContent>
            {{ $d(HHEvent.start, 'time') }} - {{ $d(HHEvent.end, 'time') }}
          </template>
        </UiDataCell>
      </AppHHTooltip>

      <USeparator orientation="vertical" class="h-8" />

      <UButton
        size="xl"
        color="secondary"
        variant="subtle"
        icon="crpg:arrow-up"
        @click="scrollToTop"
      />
    </div>
  </footer>
</template>
