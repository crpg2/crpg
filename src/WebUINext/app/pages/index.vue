<script setup lang="ts">
definePageMeta({
  layout: 'empty',
})

const { loadPatchNotes, patchNotes } = usePatchNotes()
const { gameServerStats, loadGameServerStats } = useGameServerStats()
const { loadSettings } = useSettingsStore()

loadPatchNotes()
loadGameServerStats()
loadSettings()
</script>

<template>
  <div
    class="
      relative p-4
      lg:h-screen lg:p-8
    "
  >
    <AppBg bg="background-1.webp" />

    <div
      class="
        relative flex h-full flex-col items-center justify-center gap-10 p-4 ring ring-default
        lg:p-6
      "
    >
      <AppPatchNotes
        v-if="patchNotes.length !== 0"
        class="
          top-6 left-6
          lg:absolute
        "
        :patch-notes="patchNotes"
      />

      <div
        class="
          top-6 right-6 flex items-center gap-3
          lg:absolute
        "
      >
        <AppOnlinePlayers
          :game-server-stats="gameServerStats"
          show-label
        />

        <UButton
          variant="link"
          size="xl"
          :to="{ name: 'leaderboard' }"
          icon="crpg:trophy-cup" :label="$t('nav.main.Leaderboard')"
        />

        <AppSwitchLanguageDropdown v-slot="{ open, locale }">
          <UButton
            size="lg"
            color="neutral"
            variant="ghost"
            :leading-icon="`crpg:${locale}`"
            :trailing-icon="open ? 'crpg:chevron-up' : 'crpg:chevron-down'"
            active-variant="soft"
            :active="open"
            :label="locale.toUpperCase()"
          />
        </AppSwitchLanguageDropdown>
      </div>

      <div
        class="
          mx-auto flex w-full flex-col items-center justify-center gap-8
          lg:w-1/2
        "
      >
        <UiHeading>
          <UiSpriteSymbol
            name="logo"
            viewBox="0 0 162 124"
            class="
              w-28
              2xl:w-32
            "
          />
        </UiHeading>

        <div
          class="max-w-2xl text-center"
        >
          <UiTextView tag="h1" variant="h2">
            <i18n-t
              keypath="homePage.intro"
              scope="global"
            >
              <template #link>
                <ULink
                  href="https://store.steampowered.com/app/261550/Mount__Blade_II_Bannerlord"
                  target="_blank"
                >
                  Mount & Blade II: Bannerlord
                </ULink>
              </template>
            </i18n-t>
          </UiTextView>

          <UiTextView tag="p" variant="p">
            {{ $t('homePage.description') }}
          </UiTextView>

          <iframe
            class="
              mx-auto block aspect-video w-3/4 rounded-lg
              xl:w-2/3
            "
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
              color="neutral"
              variant="subtle"
              size="xl"
              icon="crpg:download"
              :label="$t('installation.title')"
            />
          </AppInstallationGuide>
        </div>

        <AppSocials
          class="
            bottom-6 left-6
            lg:absolute
          "
        />
      </div>
    </div>
  </div>
</template>
