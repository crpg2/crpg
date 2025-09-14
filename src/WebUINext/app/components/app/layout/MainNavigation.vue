<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

import type { PatchNote } from '~/models/patch-note'

import { useSettingsStore } from '~/stores/settings'
import { useUserStore } from '~/stores/user'

defineProps<{ latestPatch?: PatchNote }>()

const userStore = useUserStore()
const { t } = useI18n()

const { settings } = storeToRefs(useSettingsStore())

const items = computed(() => [
  [
    ...(userStore.user
      ? [
          {
            label: t('nav.main.Characters'),
            to: {
              name: 'characters',
            },
          },
          {
            label: t('nav.main.Shop'),
            to: {
              name: 'shop',
            },
          },
          {
            label: t('nav.main.Clans'),
            to: {
              name: 'clans',
            },
            slot: 'clans' as const,
          },
        ]
      : []),
    {
      label: t('nav.main.Leaderboard'),
      icon: 'crpg:trophy-cup',
      to: {
        name: 'leaderboard',
      },
      slot: 'leaderboard' as const,
    },
  ],
] satisfies NavigationMenuItem[])
</script>

<template>
  <UNavigationMenu color="neutral" variant="link" :items>
    <template #list-leading>
      <UFieldGroup>
        <UTooltip
          v-if="latestPatch"
          :text="$t('patchNotes.latestPatch')"
        >
          <UButton
            variant="outline"
            icon="crpg:trumpet"
            :to="latestPatch.url"
            target="_blank"
          >
            <UBadge
              :label="latestPatch.tagName"
              size="sm"
              variant="soft"
            />
          </UButton>
        </UTooltip>

        <UTooltip :text="$t('nav.main.Community')">
          <UButton
            variant="outline"
            icon="crpg:discord"
            :to="settings.discord"
            target="_blank"
          />
        </UTooltip>

        <AppInstallationGuide>
          <UTooltip :text="$t('nav.main.Installation')">
            <UButton
              variant="outline"
              icon="crpg:download"
            />
          </UTooltip>
        </AppInstallationGuide>

        <UTooltip :text="$t('help.title')">
          <UButton
            variant="outline"
            icon="crpg:help-circle"
            :to="{ name: 'help' }"
          />
        </UTooltip>

        <UTooltip :text="$t('nav.main.Builder')">
          <UButton
            variant="outline"
            icon="crpg:calculator"
            :to="{ name: 'builder' }"
          />
        </UTooltip>
      </UFieldGroup>
    </template>

    <template #leaderboard-leading>
      <UIcon name="crpg:trophy-cup" class="size-6 text-gold" />
    </template>

    <template v-if="!userStore.clan" #clans-leading>
      <UTooltip
        data-aq-main-nav-link-tooltip="Explanation"
        :ui="{ content: 'w-72' }"
      >
        <UBadge
          icon="crpg:tag"
          variant="soft"
          size="sm"
        />
        <template #content>
          <div
            class="prose"
            v-html="$t('clanBalancingExplanation')"
          />
        </template>
      </UTooltip>
    </template>
  </UNavigationMenu>
</template>
