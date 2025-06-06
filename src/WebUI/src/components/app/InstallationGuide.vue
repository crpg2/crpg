<script setup lang="ts">
import { usePlatform } from '~/composables/use-platform'
import { Platform } from '~/models/platform'
import { platformToIcon } from '~/services/platform-service'

const { platform } = usePlatform()

enum PossibleValues {
  Steam = 'Steam',
  Other = 'Other',
}

const tabsModel = ref<PossibleValues>(PossibleValues.Steam)

watch(
  platform,
  () => {
    tabsModel.value
      = platform.value === Platform.Steam ? PossibleValues.Other : PossibleValues.Steam
  },
  {
    immediate: true,
  },
)
</script>

<template>
  <Modal closable>
    <slot>
      <OButton
        variant="secondary"
        size="xl"
        icon-left="download"
        target="_blank"
        :label="$t('installation.title')"
      />
    </slot>

    <template #popper>
      <div class="prose prose-invert space-y-6 px-12 py-10">
        <h3 class="text-center">
          {{ $t('installation.title') }}
        </h3>

        <OTabs
          v-model="tabsModel"
          size="xl"
          :animation="['fade', 'fade']"
        >
          <OTabItem :value="PossibleValues.Other">
            <template #header>
              <OIcon
                :icon="platformToIcon[Platform.Steam]"
                size="xl"
              />
              <OIcon
                :icon="platformToIcon[Platform.EpicGames]"
                size="xl"
              />
              {{ $t('installation.platform.other.title') }}
            </template>
            <ol>
              <i18n-t
                scope="global"
                keypath="installation.platform.other.downloadLauncher"
                tag="li"
              >
                <template #launcherLink>
                  <a
                    target="_blank"
                    href="https://c-rpg.eu/LauncherV3.exe"
                  >Launcher</a>
                </template>
              </i18n-t>
              <li>{{ $t('installation.platform.other.startLauncher') }}</li>
              <li>{{ $t('installation.platform.other.detectinstall') }}</li>
              <li>{{ $t('installation.platform.other.update') }}</li>
              <li>{{ $t('installation.platform.other.launch') }}</li>
            </ol>
          </OTabItem>

          <OTabItem
            label="Steam Workshop"
            :icon="platformToIcon[Platform.Steam]"
            :value="PossibleValues.Steam"
          >
            <ol>
              <i18n-t
                scope="global"
                keypath="installation.platform.steam.subscribe"
                tag="li"
              >
                <template #steamWorkshopsLink>
                  <a
                    target="_blank"
                    href="steam://openurl/https://steamcommunity.com/sharedfiles/filedetails/?id=2878356589"
                  >Steam Workshop</a>
                </template>
              </i18n-t>
              <li>{{ $t('installation.platform.steam.bannerlordLauncher') }}</li>
              <li>{{ $t('installation.platform.steam.multiplayerModsTab') }}</li>
              <li>{{ $t('installation.platform.steam.activateMod') }}</li>
              <li>{{ $t('installation.platform.steam.launchMultiplayerGame') }}</li>
            </ol>
            <p class="text-primary">
              {{ $t('installation.platform.steam.update') }}
            </p>
          </OTabItem>
        </OTabs>

        <div class="flex justify-center">
          <OButton
            variant="primary"
            size="xl"
            icon-left="youtube"
            target="_blank"
            outlined
            tag="a"
            href="https://www.youtube.com/watch?v=F2NMyFAAev0"
            :label="$t('installation.common.watchVideoGuide')"
          />
        </div>

        <Divider />

        <div class="mt-6 space-y-6">
          <i18n-t
            scope="global"
            keypath="installation.common.help"
            tag="p"
            class="text-content-400"
          >
            <template #discordLink>
              <!-- prettier-ignore -->
              <a
                target="_blank"
                href="https://discord.com/channels/279063743839862805/761283333840699392"
              >Discord</a>
            </template>
          </i18n-t>
        </div>
      </div>
    </template>
  </Modal>
</template>
