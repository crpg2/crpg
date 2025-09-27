<script setup lang="ts">
import { useIntervalFn } from '@vueuse/core'

import { useMainHeaderProvider } from '~/composables/app/use-main-header'

const route = useRoute()
const userStore = useUserStore()

const { data: patchNotes } = usePatchNotes()
const { data: gameServerStats } = useGameServerStats()

const { hHEvent, HHEventRemaining, isHHCountdownEnded } = useHappyHours()

userStore.fetchUserRestriction()

usePollInterval({
  key: 'user',
  fn: userStore.fetchUser,
})

useMainHeaderProvider(useTemplateRef('mainHeader'))

const { showWelcomeMessage } = useWelcome()

const { pause, resume, isActive } = useIntervalFn(() => {
  hHEvent.trigger()
}, 1000)
</script>

<template>
  <div class="relative flex min-h-[calc(100vh+1px)] flex-col">
    <AppBg
      v-if="route.meta?.layoutOptions?.bg"
      :bg="route.meta.layoutOptions.bg"
    />

    <header
      ref="mainHeader"
      class="ring ring-default"
      :class="[
        !route.meta.layoutOptions?.noStickyHeader ? `
          sticky top-0 z-20 bg-default/10 backdrop-blur-sm
        ` : `bg-default`]"
    >
      <template v-if="userStore.user">
        <LazyUserRestrictionBanner
          v-if="userStore.restriction"
          :restriction="userStore.restriction"
        />

        <pre>
          {{ {
            hHEvent,
            HHEventRemaining,
            isHHCountdownEnded,
          } }}
        </pre>

        <LazyAppHHBanner
          v-if="!isHHCountdownEnded && HHEventRemaining !== 0"
          :region="userStore.user.region"
          :h-h-event
        />
      </template>

      <div class="flex flex-wrap items-center justify-between p-3">
        <div class="flex items-center gap-4">
          <NuxtLink :to="{ name: 'index' }">
            <UIcon
              name="crpg:logo"
              class="size-14 text-highlighted"
            />
          </NuxtLink>

          <AppOnlinePlayers :game-server-stats="gameServerStats" />

          <AppLayoutMainNavigation :latest-patch="patchNotes[0]" />
        </div>

        <UserHeaderToolbar
          v-if="userStore.user"
          :user="userStore.user"
          @show-welcome="showWelcomeMessage"
        />

        <AppLayoutPublicHeaderToolbar v-else />
      </div>
    </header>

    <main class="relative flex-1">
      <slot />
    </main>

    <!-- TODO: FIXME: - Стратегус v-if="!route.meta.noFooter" -->
    <AppLayoutFooter :h-h-event class="ring ring-default" />
  </div>
</template>
