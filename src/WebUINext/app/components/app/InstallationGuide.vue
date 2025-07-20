<script setup lang="ts">
import type { TabsItem } from '@nuxt/ui'

import { PLATFORM } from '~/models/platform'
import { platformToIcon } from '~/services/platform-service'

enum PossibleValue {
  Steam = 'Steam',
  Other = 'Other',
}

const tabsModel = ref<PossibleValue>(PossibleValue.Other)
const { t } = useI18n()

const items = ref<TabsItem[]>([
  {
    label: t('installation.platform.other.title'),
    value: PossibleValue.Other,
  },
  {
    label: 'Steam Workshop',
    value: PossibleValue.Steam,
  },
])
</script>

<template>
  <UModal :title="$t('installation.title')">
    <slot />

    <template #body>
      <UTabs
        v-model="tabsModel"
        :items
        size="xl"
        variant="pill"
      >
        <template #leading="{ item }">
          <template v-if="item.value === PossibleValue.Other">
            <UIcon
              :name="`crpg:${platformToIcon[PLATFORM.Steam]}`"
              class="size-6"
            />
            <UIcon
              :name="`crpg:${platformToIcon[PLATFORM.Microsoft]}`"
              class="size-6"
            />
            <UIcon
              :name="`crpg:${platformToIcon[PLATFORM.EpicGames]}`"
              class="size-6"
            />
          </template>
          <UIcon
            v-else
            :name="`crpg:${platformToIcon[PLATFORM.Steam]}`"
            class="size-6"
          />
        </template>

        <template #content="{ item }">
          <div class="prose">
            <template v-if="item.value === PossibleValue.Other">
              <ol>
                <i18n-t
                  scope="global"
                  keypath="installation.platform.other.downloadLauncher"
                  tag="li"
                >
                  <template #launcherLink>
                    <NuxtLink
                      target="_blank"
                      href="https://c-rpg.eu/LauncherV3.exe"
                    >
                      Launcher
                    </NuxtLink>
                  </template>
                </i18n-t>
                <li>{{ $t('installation.platform.other.startLauncher') }}</li>
                <li>{{ $t('installation.platform.other.detectinstall') }}</li>
                <li>{{ $t('installation.platform.other.update') }}</li>
                <li>{{ $t('installation.platform.other.launch') }}</li>
              </ol>
            </template>

            <template v-else>
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
            </template>
          </div>
        </template>
      </UTabs>

      <div class="mt-4 flex justify-center">
        <UButton
          variant="outline"
          size="xl"
          icon="crpg:youtube"
          to="https://www.youtube.com/watch?v=F2NMyFAAev0"
          target="_blank"
          :label="$t('installation.common.watchVideoGuide')"
        />
      </div>
    </template>

    <template #footer>
      <div class="prose">
        <i18n-t
          scope="global"
          keypath="installation.common.help"
          tag="p"
        >
          <template #discordLink>
            <NuxtLink
              target="_blank"
              href="https://discord.com/channels/279063743839862805/761283333840699392"
            >
              Discord
            </NuxtLink>
          </template>
        </i18n-t>
      </div>
    </template>
  </UModal>
</template>
