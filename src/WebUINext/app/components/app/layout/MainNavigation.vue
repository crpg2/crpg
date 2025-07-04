<script setup lang="ts">
import type { PatchNote } from '~/models/patch-note'

import { useSettingsStore } from '~/stores/settings'
import { useUserStore } from '~/stores/user'

defineProps<{ latestPatch?: PatchNote }>()

const userStore = useUserStore()
const { settings } = storeToRefs(useSettingsStore())
</script>

<template>
  <nav class="flex items-center gap-5">
    <UButtonGroup size="sm">
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
            size="xs"
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
    </UButtonGroup>

    <template v-if="userStore.user">
      <ULink
        :to="{ name: 'characters' }"
        active-class="text-content-100"
      >
        {{ $t('nav.main.Characters') }}
      </ULink>

      <ULink
        :to="{ name: 'shop' }"
        active-class="text-highlighted"
      >
        {{ $t('nav.main.Shop') }}
      </ULink>

      <div class="flex items-center gap-1.5">
        <UTooltip
          v-if="!userStore.clan"
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
              class="prose prose-invert"
              v-html="$t('clanBalancingExplanation')"
            />
          </template>
        </UTooltip>

        <ULink

          :to="{ name: 'clans' }"
          active-class="text-highlighted"
        >
          {{ $t('nav.main.Clans') }}
        </ULink>
      </div>
    </template>

    <ULink
      :to="{ name: 'leaderboard' }"
      class="flex gap-1.5"
      active-class="text-highlighted"
    >
      <UIcon
        name="crpg:trophy-cup"
        class="size-6 text-more-support"
      />
      {{ $t('nav.main.Leaderboard') }}
    </ULink>
  </nav>
</template>
