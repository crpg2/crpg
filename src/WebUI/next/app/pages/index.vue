<script setup lang="ts">
import { useSettingsStore } from '~/stores/settings'

definePageMeta({
  layout: 'empty',
  skipAuth: true,
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
        class="absolute left-6 top-6"
        :patch-notes="patchNotes"
      />

      <div class="absolute right-6 top-6 flex items-center gap-6">
        <AppOnlinePlayers
          :game-server-stats="gameServerStats"
          show-label
        />

        <AppSwitchLanguageDropdown>
          <template #default="{ shown, locale }">
            <OButton
              :variant="shown ? 'transparent-active' : 'transparent'"
              size="sm"
            >
              <span class="text-xs font-normal">{{ locale.toUpperCase() }}</span>
              <div class="flex items-center gap-2.5">
                <SpriteSymbol
                  :key="locale"
                  :name="`locale/${locale}`"
                  viewBox="0 0 18 18"
                  inline
                  class="w-4"
                />
                <div class="h-4 w-px select-none bg-border-300" />
                <OIcon
                  icon="chevron-down"
                  size="lg"
                  :rotation="shown ? 180 : 0"
                  class="text-content-400"
                />
              </div>
            </OButton>
          </template>
        </AppSwitchLanguageDropdown>
      </div>

      <div class="mx-auto flex flex-col items-center justify-center gap-14 md:w-1/2 2xl:w-1/3">
        <div class="space-y-6">
          <div class="flex select-none items-center justify-center gap-6 md:gap-12">
            <SpriteSymbol
              name="logo-decor"
              viewBox="0 0 108 10"
              inline
              class="w-24 rotate-180"
            />
            <SpriteSymbol
              name="logo"
              viewBox="0 0 162 124"
              inline
              class="w-24 xl:w-28 2xl:w-32"
            />
            <SpriteSymbol
              name="logo-decor"
              viewBox="0 0 108 10"
              inline
              class="w-24"
            />
          </div>
        </div>

        <div class="prose prose-invert text-center">
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
          <AppInstallationGuide />
        </div>

        <AppSocials class="absolute bottom-6 left-6" />
      </div>
    </div>
  </div>
</template>
