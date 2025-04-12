<script setup lang="ts">
import { useElementSize } from '@vueuse/core'

import { useGameServerStats } from '~/composables/use-game-server-stats'
import { useHappyHours } from '~/composables/use-hh'
import { usePatchNotes } from '~/composables/use-patch-notes'
import { usePollInterval } from '~/composables/use-poll-interval'
import { useSettingsStore } from '~/stores/settings'
import { useUserStore } from '~/stores/user'
import { mainHeaderHeightKey } from '~/symbols/common'

const userStore = useUserStore()
const settingsStore = useSettingsStore()

const fetchUserPollId = Symbol('fetchUser')

const route = useRoute()

const { loadPatchNotes, patchNotes } = usePatchNotes()
const { HHEvent, HHEventRemaining, HHPollId, isHHCountdownEnded } = useHappyHours()
const { gameServerStats, loadGameServerStats } = useGameServerStats()

const promises: Array<Promise<any>> = [
  loadPatchNotes(),
  loadGameServerStats(),
  userStore.fetchUserRestriction(),
  settingsStore.loadSettings(),
]

const mainHeader = ref<HTMLDivElement | null>(null)
const { height: mainHeaderHeight } = useElementSize(
  mainHeader,
  { height: 0, width: 0 },
  { box: 'border-box' },
)
provide(mainHeaderHeightKey, mainHeaderHeight)

const { subscribe, unsubscribe } = usePollInterval()

subscribe(fetchUserPollId, userStore.fetchUser)
subscribe(HHPollId, HHEvent.trigger)

onBeforeUnmount(() => {
  unsubscribe(fetchUserPollId)
  unsubscribe(HHPollId)
})

await Promise.all(promises)
</script>

<template>
  <div class="relative flex min-h-[calc(100vh+1px)] flex-col">
    <Bg
      v-if="route.meta?.bg"
      :bg="(route.meta.bg as string)"
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

      <HHHeaderNotification v-if="!isHHCountdownEnded && HHEventRemaining !== 0" />

      <div class="flex flex-wrap items-center justify-between p-3">
        <div class="flex items-center gap-4">
          <RouterLink :to="{ name: 'Root' }">
            <SvgSpriteImg
              name="logo"
              viewBox="0 0 162 124"
              class="w-14"
            />
          </RouterLink>

          <OnlinePlayers :game-server-stats="gameServerStats" />

          <Divider inline />

          <MainNavigation :latest-patch="patchNotes[0]" />
        </div>

        <UserHeaderToolbar />
      </div>
    </header>

    <main class="relative flex-1">
      <RouterView />
    </main>

    <Footer
      v-if="!route.meta.noFooter"
      :HHEvent="HHEvent"
    />
  </div>
</template>
