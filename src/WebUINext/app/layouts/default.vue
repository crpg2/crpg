<script setup lang="ts">
const route = useRoute()
const userStore = useUserStore()

const mainHeaderRef = useTemplateRef('mainHeader')

const { loadPatchNotes, patchNotes } = usePatchNotes()
const { gameServerStats, loadGameServerStats } = useGameServerStats()
const settingsStore = useSettingsStore()
const { HHEvent, HHEventRemaining, HHPollId, isHHCountdownEnded } = useHappyHours()

Promise.all([
  loadPatchNotes(),
  loadGameServerStats(),
  settingsStore.loadSettings(),

  // userStore.fetchUserRestriction(),
])
</script>

<template>
  <div class="relative flex min-h-[calc(100vh+1px)] flex-col">
    <AppBg
      v-if="route.meta?.layoutOptions?.bg"
      :bg="route.meta.layoutOptions.bg"
    />

    <header
      ref="mainHeader"
      class="z-20 border-b border-solid border-border-200 bg-bg-main"
      :class="{ 'sticky top-0 bg-bg-main/10 backdrop-blur-sm': !route.meta?.noStickyHeader }"
    >
      <UserRestrictionNotification
        v-if="userStore.restriction !== null"
        :restriction="userStore.restriction"
      />

      <AppLayoutHHHeaderNotification v-if="!isHHCountdownEnded && HHEventRemaining !== 0" />

      <div class="flex flex-wrap items-center justify-between p-3">
        <div class="flex items-center gap-4">
          <NuxtLink :to="{ name: 'index' }">
            <SpriteSymbol
              name="logo"
              inline
              viewBox="0 0 162 124"
              class="w-14"
            />
          </NuxtLink>

          <AppOnlinePlayers :game-server-stats="gameServerStats" />

          <UiDivider inline />

          <AppLayoutMainNavigation :latest-patch="patchNotes[0]" />
        </div>

        <UserHeaderToolbar
          v-if="userStore.user"
          :user="userStore.user"
        />

        <AppLogin v-else size="sm" />
      </div>
    </header>

    <main class="relative flex-1">
      <slot />
    </main>

    <!-- TODO: v-if="!route.meta.noFooter" -->
    <AppLayoutFooter :HHEvent />
  </div>
</template>
