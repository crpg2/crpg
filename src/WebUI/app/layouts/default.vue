<script setup lang="ts">
import { useMainHeaderProvider } from '~/composables/app/use-main-header'
import { useUser } from '~/composables/user/use-user'
import { useUserRestriction } from '~/composables/user/use-user-restriction'

const route = useRoute()

const { data: patchNotes } = usePatchNotes()
const { data: gameServerStats } = useGameServerStats()

const { hHEvent, isHhEventActive, onEndHH } = useHappyHours()

const { restriction, fetchUserRestriction } = useUserRestriction()
const { user, fetchUser } = useUser()

usePollInterval({
  key: 'user',
  fn: fetchUser,
})

useMainHeaderProvider(useTemplateRef('mainHeader'))
const { showWelcomeMessage } = useWelcome()

if (user.value) {
  fetchUserRestriction()
}
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
      <LazyUserRestrictionBanner
        v-if="user && restriction"
        :restriction
      />

      <div class="flex flex-wrap items-center justify-between p-3">
        <div class="flex flex-wrap items-center gap-3">
          <NuxtLink :to="{ name: 'index' }">
            <UIcon
              name="crpg:logo"
              class="size-14 text-highlighted"
            />
          </NuxtLink>

          <AppOnlinePlayers :game-server-stats="gameServerStats" />

          <LazyAppHHBadge
            v-if="user && isHhEventActive"
            :region="user.region"
            :h-h-event
            @complete="onEndHH"
          />

          <AppLayoutMainNavigation :latest-patch="patchNotes[0]" />
        </div>

        <UserHeaderToolbar
          v-if="user"
          :user
          @show-welcome="showWelcomeMessage"
        />

        <AppLayoutPublicHeaderToolbar v-else />
      </div>
    </header>

    <main class="relative flex-1">
      <slot />
    </main>

    <AppLayoutFooter v-if="!route.meta.layoutOptions?.noFooter" :h-h-event class="ring ring-default" />
  </div>
</template>
