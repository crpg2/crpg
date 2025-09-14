<script setup lang="ts">
import { useMainHeaderProvider } from '~/composables/app/use-main-header'

const route = useRoute()
const userStore = useUserStore()

const { loadPatchNotes, patchNotes } = usePatchNotes()
const { gameServerStats, loadGameServerStats } = useGameServerStats()
const settingsStore = useSettingsStore()
const { HHEvent, HHEventRemaining, HHPollId, isHHCountdownEnded } = useHappyHours()

await Promise.all([
  loadPatchNotes(),
  loadGameServerStats(),
  settingsStore.loadSettings(),
  userStore.fetchUserRestriction(),
])

// TODO: FIXME:
useWelcome()

const mainHeader = useTemplateRef('mainHeader')
const { height: mainHeaderHeight } = useElementSize(mainHeader, { height: 0, width: 0 }, { box: 'border-box' })
useMainHeaderProvider(mainHeaderHeight)
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
        <!-- TODO: тест -->
        <UserRestrictionNotification
          v-if="userStore.restriction !== null"
          :restriction="userStore.restriction"
        />

        <!-- v-if="userStore.user && !isHHCountdownEnded && HHEventRemaining !== 0" -->
        <!-- TODO: тест -->
        <AppLayoutHHHeaderNotification
          :region="userStore.user!.region"
        />
      </template>

      <div class="flex flex-wrap items-center justify-between p-3">
        <div class="flex items-center gap-4">
          <NuxtLink :to="{ name: 'index' }">
            <UIcon
              name="crpg:logo"
              class="size-14"
            />
          </NuxtLink>

          <AppOnlinePlayers :game-server-stats="gameServerStats" />

          <AppLayoutMainNavigation :latest-patch="patchNotes[0]" />
        </div>

        <UserHeaderToolbar
          v-if="userStore.user"
          :user="userStore.user"
        />

        <AppLayoutPublicHeaderToolbar v-else />
      </div>
    </header>

    <main class="relative flex-1">
      <slot />
    </main>

    <!-- TODO: v-if="!route.meta.noFooter" -->
    <AppLayoutFooter :HHEvent class="ring ring-default" />

    <!-- :open="shownWelcomeMessage" -->
    <!-- TODO: FIXME: -->
  </div>
</template>
