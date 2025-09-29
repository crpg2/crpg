<script setup lang="ts">
import { useMainHeaderProvider } from '~/composables/app/use-main-header'

const route = useRoute()
const userStore = useUserStore()

const { data: patchNotes } = usePatchNotes()
const { data: gameServerStats } = useGameServerStats()

const { hHEvent, isHhEventActive, onEndHH } = useHappyHours()

userStore.fetchUserRestriction()

usePollInterval({
  key: 'user',
  fn: userStore.fetchUser,
})

useMainHeaderProvider(useTemplateRef('mainHeader'))
const { showWelcomeMessage } = useWelcome()
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
        <LazyAppHHBanner
          v-if="isHhEventActive"
          :region="userStore.user.region"
          :h-h-event
          @complete="onEndHH"
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
