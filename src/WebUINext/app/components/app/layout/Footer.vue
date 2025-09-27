<script setup lang="ts">
import type { HHEvent } from '~/services/hh-service'

import { useUserStore } from '~/stores/user'

defineProps<{ hHEvent: HHEvent }>()

const userStore = useUserStore()

const scrollToTop = () =>
  window.scrollTo({
    behavior: 'smooth',
    top: 0,
  })
</script>

<template>
  <footer
    class="relative mt-auto flex flex-wrap items-center justify-between px-6 py-5 text-xs"
  >
    <AppSocials
      patreon-expanded
      size="md"
    />

    <div class="flex items-center gap-5">
      <template v-if="userStore.user">
        <AppHHTooltip :region="userStore.user.region">
          <UiDataCell class="hover:text-highlighted">
            <template #leftContent>
              <UIcon
                name="crpg:gift"
                class="size-6"
              />
            </template>
            {{ $t('hh.tooltip-trigger', { region: $t(`region.${userStore.user.region}`, 1) }) }}
            <template #rightContent>
              {{ $d(hHEvent.start, 'time') }} - {{ $d(hHEvent.end, 'time') }}
            </template>
          </UiDataCell>
        </AppHHTooltip>

        <USeparator orientation="vertical" class="h-8" />
      </template>

      <UButton
        size="xl"
        color="neutral"
        variant="outline"
        icon="crpg:arrow-up"
        @click="scrollToTop"
      />
    </div>
  </footer>
</template>
