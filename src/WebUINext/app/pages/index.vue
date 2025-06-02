<script setup lang="ts">
import { useSettingsStore } from '~/stores/settings'

definePageMeta({
  layout: 'empty',
})

const { loadPatchNotes, patchNotes } = usePatchNotes()
const { gameServerStats, loadGameServerStats } = useGameServerStats()
const { loadSettings } = useSettingsStore()

Promise.all([
  loadPatchNotes(),
  loadGameServerStats(),
  loadSettings(),
])
</script>

<template>
  <div class="relative h-screen p-4 md:p-8">
    <AppBg bg="background-1.webp" />

    <div class="relative flex h-full items-center border border-border-300 text-content-200">
      <AppPatchNotes
        v-if="patchNotes.length !== 0"
        class="absolute top-6 left-6"
        :patch-notes="patchNotes"
      />

      <div class="absolute top-6 right-6 flex items-center gap-6">
        <AppOnlinePlayers
          :game-server-stats="gameServerStats"
          show-label
        />

        <NuxtLink
          :to="{ name: 'leaderboard' }"
          class="inline-flex items-center gap-1.5 text-content-300 hover:text-content-100"
          active-class="!text-content-100"
        >
          <OIcon
            icon="trophy-cup"
            size="xl"
            class="text-more-support"
          />
          {{ $t('nav.main.Leaderboard') }}
        </NuxtLink>

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

      <div class="mx-auto flex flex-col items-center justify-center gap-14 md:w-1/2 2xl:w-1/3">
        <div class="space-y-6">
          <!-- TODO: to cmp -->
          <div class="flex items-center justify-center gap-6 select-none md:gap-12">
            <UiSpriteSymbol
              name="logo-decor"
              class="w-24 rotate-180"
            />
            <UiSpriteSymbol
              name="logo"
              class="w-24 fill-current text-white xl:w-28 2xl:w-32"
            />
            <UiSpriteSymbol
              name="logo-decor"
              class="w-24"
            />
          </div>
        </div>

        <div class="prose text-center prose-invert">
          <i18n-t
            keypath="homePage.intro"
            tag="p"
            scope="global"
          >
            <template #link>
              <a
                class="text-content-link hover:text-content-link-hover"
                href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord"
                target="_blank"
              >
                Mount & Blade II: Bannerlord.
              </a>
            </template>
          </i18n-t>
          <p>
            {{ $t('homePage.description') }}
          </p>

          <iframe
            class="mx-auto block aspect-video w-full rounded-lg lg:w-3/4 xl:w-2/3"
            src="https://www.youtube-nocookie.com/embed/MQnW_j6s1Jw"
            title="YouTube video player"
            frameborder="0"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
            allowfullscreen
          />
        </div>

        <div class="flex justify-center gap-4">
          <AppLogin />
          <AppInstallationGuide>
            <UButton
              color="secondary"
              size="xl"
              icon="crpg:download"
              :label="$t('installation.title')"
            />
          </AppInstallationGuide>
        </div>

        <AppSocials class="absolute bottom-6 left-6" />
      </div>
    </div>
  </div>
</template>
