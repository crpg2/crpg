<script setup lang="ts">
import { useMainHeaderProvider } from '~/composables/app/use-main-header'

const route = useRoute('index') // index for stub, so TS doesn't swear
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
        !route.meta.layoutOptions?.noStickyHeader
          ? `sticky top-0 z-20 bg-default/10 backdrop-blur-sm` : `bg-default`]"
    >
      <UserRestrictionNotification
        v-if="userStore.user && userStore.restriction !== null"
        :restriction="userStore.restriction"
      />

      <!-- v-if="userStore.user && !isHHCountdownEnded && HHEventRemaining !== 0" -->
      <AppLayoutHHHeaderNotification
        v-if="userStore.user"
        :region="userStore.user!.region"
      />

      <div class="flex flex-wrap items-center justify-between p-3">
        <div class="flex items-center gap-4">
          <NuxtLink :to="{ name: 'index' }">
            <UiSpriteSymbol
              name="logo"
              viewBox="0 0 162 124"
              class="w-14"
            />
          </NuxtLink>

          <AppOnlinePlayers :game-server-stats="gameServerStats" />

          <USeparator orientation="vertical" class="h-6" />

          <AppLayoutMainNavigation :latest-patch="patchNotes[0]" />
        </div>

        <UserHeaderToolbar
          v-if="userStore.user"
          :user="userStore.user"
        />

        <!-- TODO: to cmp PublicHeaderToolbar -->
        <div v-else class="flex items-center gap-4">
          <AppLogin size="sm" />

          <AppSwitchLanguageDropdown v-slot="{ open, locale }">
            <UButton
              size="lg"
              color="secondary"
              variant="ghost"
              :leading-icon="`crpg:${locale}`"
              :trailing-icon="open ? 'crpg:chevron-up' : 'crpg:chevron-down'"
              active-variant="solid"
              :active="open"
              :label="locale.toUpperCase()"
            />
          </AppSwitchLanguageDropdown>
        </div>
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
