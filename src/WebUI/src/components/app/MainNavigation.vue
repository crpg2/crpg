<script setup lang="ts">
import type { PatchNote } from '~/models/patch-note'

import Role from '~/models/role'
import { useSettingsStore } from '~/stores/settings'
import { useUserStore } from '~/stores/user'

defineProps<{ latestPatch?: PatchNote }>()

const userStore = useUserStore()
const { settings } = storeToRefs(useSettingsStore())
</script>

<template>
  <nav class="flex items-center gap-5">
    <div class="flex items-center rounded-full border border-border-200 hover:border-border-300">
      <OButton
        v-if="latestPatch"
        v-tooltip.bottom="$t('patchNotes.latestPatch')"
        variant="primary"
        class="cursor-pointer"
        size="sm"
        inverted
        tag="a"
        icon-left="trumpet"
        :href="latestPatch.url"
        target="_blank"
      >
        <Tag
          variant="primary"
          :label="latestPatch.tagName"
        />
      </OButton>

      <OButton
        v-tooltip.bottom="$t('nav.main.Community')"
        variant="primary"
        size="sm"
        inverted
        rounded
        tag="a"
        icon-left="discord"
        :href="settings.discord"
        target="_blank"
      />

      <InstallationGuide>
        <OButton
          v-tooltip.bottom="$t('nav.main.Installation')"
          variant="primary"
          inverted
          rounded
          size="sm"
          icon-left="download"
        />
      </InstallationGuide>

      <RouterLink :to="{ name: 'Help' }">
        <OButton
          v-tooltip.bottom="$t('help.title')"
          variant="primary"
          size="sm"
          inverted
          rounded
          icon-left="help-circle"
        />
      </RouterLink>
    </div>

    <RouterLink
      :to="{ name: 'Characters' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
    >
      {{ $t('nav.main.Characters') }}
    </RouterLink>

    <RouterLink
      :to="{ name: 'Shop' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
    >
      {{ $t('nav.main.Shop') }}
    </RouterLink>

    <div class="flex items-center gap-1.5">
      <VTooltip
        v-if="userStore.clan === null"
        data-aq-main-nav-link-tooltip="Explanation"
      >
        <Tag
          icon="tag"
          variant="primary"
          rounded
          size="sm"
        />
        <template #popper>
          <div
            class="prose prose-invert"
            v-html="$t('clanBalancingExplanation')"
          />
        </template>
      </VTooltip>

      <RouterLink
        :to="{ name: 'Clans' }"
        class="text-content-300 hover:text-content-100"
        active-class="!text-content-100"
      >
        {{ $t('nav.main.Clans') }}
      </RouterLink>
    </div>

    <RouterLink
      :to="{ name: 'Leaderboard' }"
      class="inline-flex items-center gap-1.5 text-content-300 hover:text-content-100"
      active-class="!text-content-100"
    >
      <OIcon
        icon="trophy-cup"
        size="xl"
        class="text-more-support"
      />
      {{ $t('nav.main.Leaderboard') }}
    </RouterLink>

    <RouterLink
      :to="{ name: 'Credits' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
    >
      {{ $t('nav.main.Credits') }}
</RouterLink>


    <RouterLink
      v-if="[Role.Moderator, Role.Admin].includes(userStore.user!.role)"
      :to="{ name: 'Moderator' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
      data-aq-main-nav-link="Moderator"
    >
      {{ $t('nav.main.Moderator') }}
    </RouterLink>

    <RouterLink
      v-if="[Role.Admin].includes(userStore.user!.role)"
      :to="{ name: 'Admin' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
      data-aq-main-nav-link="Admin"
    >
      {{ $t('nav.main.Admin') }}
    </RouterLink>
  </nav>
</template>
