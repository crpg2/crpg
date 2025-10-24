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
        :ui="{
          root: 'gap-6',
        }"
      >
        <template #leading="{ item }">
          <template v-if="item.value === PossibleValue.Other">
            <UIcon
              :name="`crpg:${platformToIcon[PLATFORM.Steam]}`"
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
          <UiListView v-if="item.value === PossibleValue.Other" variant="number">
            <UiListViewItem>
              <i18n-t
                scope="global"
                keypath="installation.platform.other.downloadLauncher"
              >
                <template #launcherLink>
                  <ULink
                    target="_blank"
                    href="https://c-rpg.eu/LauncherV3.exe"
                  >
                    Launcher
                  </ULink>
                </template>
              </i18n-t>
            </UiListViewItem>
            <UiListViewItem :label="$t('installation.platform.other.startLauncher')" />
            <UiListViewItem :label="$t('installation.platform.other.detectinstall')" />
            <UiListViewItem :label="$t('installation.platform.other.update')" />
            <UiListViewItem :label="$t('installation.platform.other.detectinstall')" />
          </UiListView>

          <template v-else>
            <UiListView variant="number">
              <UiListViewItem>
                <i18n-t
                  scope="global"
                  keypath="installation.platform.steam.subscribe"
                >
                  <template #steamWorkshopsLink>
                    <ULink
                      target="_blank"
                      href="steam://openurl/https://steamcommunity.com/sharedfiles/filedetails/?id=2878356589"
                    >
                      Steam Workshop
                    </ULink>
                  </template>
                </i18n-t>
              </UiListViewItem>
              <UiListViewItem :label="$t('installation.platform.steam.bannerlordLauncher')" />
              <UiListViewItem :label="$t('installation.platform.steam.multiplayerModsTab')" />
              <UiListViewItem :label="$t('installation.platform.steam.activateMod')" />
              <UiListViewItem :label="$t('installation.platform.steam.launchMultiplayerGame')" />
            </UiListView>

            <UiTextView variant="p" tag="p" class="text-primary">
              {{ $t('installation.platform.steam.update') }}
            </UiTextView>
          </template>
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
      <UiTextView variant="p" tag="p">
        <i18n-t
          scope="global"
          keypath="installation.common.help"
        >
          <template #discordLink>
            <ULink
              target="_blank"
              href="https://discord.com/channels/279063743839862805/761283333840699392"
            >
              Discord
            </ULink>
          </template>
        </i18n-t>
      </UiTextView>
    </template>
  </UModal>
</template>
