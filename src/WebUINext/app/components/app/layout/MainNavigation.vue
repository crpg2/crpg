<script setup lang="ts">
import type { NavigationMenuItem } from '@nuxt/ui'

import type { PatchNote } from '~/models/patch-note'

import { useUserStore } from '~/stores/user'

defineProps<{ latestPatch?: PatchNote }>()

const { settings } = useAppConfig()

const userStore = useUserStore()
const { t } = useI18n()

const items = computed(() => {
  const common: NavigationMenuItem[] = [

    {
      label: t('nav.main.Leaderboard'),
      icon: 'crpg:trophy-cup',
      to: { name: 'leaderboard' },
      slot: 'leaderboard',
    },
    {
      label: t('nav.main.Credits'),
      to: { name: 'credits' },
    },
  ]

  if (!userStore.user) {
    return common
  }

  const authed: NavigationMenuItem[] = [
    { label: t('nav.main.Characters'), to: { name: 'characters' } },
    { label: t('nav.main.Shop'), to: { name: 'shop' } },
    { label: t('nav.main.Clans'), to: { name: 'clans' }, slot: 'clans' },
  ]

  return [...authed, ...common]
})
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

        <AppInstallationGuide>
          <UTooltip :text="$t('nav.main.Installation')">
            <UButton
              variant="outline"
              icon="crpg:download"
            />
          </UTooltip>
        </AppInstallationGuide>

        <UTooltip :text="$t('nav.main.Community')">
          <UButton
            variant="outline"
            icon="crpg:discord"
            :to="settings.discord"
            target="_blank"
          />
        </UTooltip>

        <UTooltip :text="$t('help.title')">
          <NuxtLink
            v-slot="{ isExactActive, isActive }"
            :to="{ name: 'help' }"
            custom
          >
            <UButton
              icon="crpg:help-circle"
              variant="outline"
              active-variant="subtle"
              :active="isExactActive || isActive"
              :to="{ name: 'help' }"
            />
          </NuxtLink>
        </UTooltip>

        <UTooltip :text="$t('nav.main.Builder')">
          <NuxtLink
            v-slot="{ isExactActive }"
            :to="{ name: 'builder' }"
            custom
          >
            <UButton
              icon="crpg:calculator"
              variant="outline"
              active-variant="subtle"
              :active="isExactActive "
              :to="{ name: 'builder' }"
            />
          </NuxtLink>
        </UTooltip>
      </UFieldGroup>
    </template>

    <template #leaderboard-leading>
      <UIcon name="crpg:trophy-cup" class="size-6 text-gold" />
    </template>

    <template v-if="!userStore.clan" #clans-leading>
      <UTooltip data-aq-main-nav-link-tooltip="Explanation">
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
