<script setup lang="ts">
const route = useRoute()
const userStore = useUserStore()

const mainHeaderRef = useTemplateRef('mainHeader')

const { loadPatchNotes, patchNotes } = usePatchNotes()
const { gameServerStats, loadGameServerStats } = useGameServerStats()

Promise.all([
  loadPatchNotes(),
  loadGameServerStats(),
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
      <!-- <UserRestrictionNotification
        v-if="userStore.restriction !== null"
        :restriction="userStore.restriction"
      /> -->

      <!-- <HHHeaderNotification v-if="!isHHCountdownEnded && HHEventRemaining !== 0" /> -->

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

          <AppMainNavigation :latest-patch="patchNotes[0]" />
        </div>

        <UserHeaderToolbar
          v-if="userStore.user"
          :user="userStore.user"
        />
      </div>
    </header>

    <main class="relative flex-1">
      <slot />
    </main>

    <!-- <Footer
      v-if="!route.meta.noFooter"
      :HHEvent="HHEvent"
    /> -->
  </div>
</template>
