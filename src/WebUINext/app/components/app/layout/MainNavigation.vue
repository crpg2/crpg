<script setup lang="ts">
import type { PatchNote } from '~/models/patch-note'

import { Role } from '~/models/role'
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
        <UiTag
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

      <AppInstallationGuide>
        <OButton
          v-tooltip.bottom="$t('nav.main.Installation')"
          variant="primary"
          inverted
          rounded
          size="sm"
          icon-left="download"
        />
      </AppInstallationGuide>

      <NuxtLink :to="{ name: 'help' }">
        <OButton
          v-tooltip.bottom="$t('help.title')"
          variant="primary"
          size="sm"
          inverted
          rounded
          icon-left="help-circle"
        />
      </NuxtLink>
    </div>

    <NuxtLink
      v-if="userStore.user"
      :to="{ name: 'characters' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
    >
      {{ $t('nav.main.Characters') }}
    </NuxtLink>

    <!--  <NuxtLink
      :to="{ name: 'Shop' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
    >
      {{ $t('nav.main.Shop') }}
    </NuxtLink> -->
    <div class="flex items-center gap-1.5">
      <VTooltip
        v-if="!userStore.clan"
        data-aq-main-nav-link-tooltip="Explanation"
      >
        <UiTag
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

      <NuxtLink
        :to="{ name: 'clans' }"
        class="text-content-300 hover:text-content-100"
        active-class="!text-content-100"
      >
        {{ $t('nav.main.Clans') }}
      </NuxtLink>
    </div>

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

    <NuxtLink
      v-if="[Role.Moderator, Role.Admin].includes(userStore.user!.role)"
      :to="{ name: 'moderator' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
      data-aq-main-nav-link="Moderator"
    >
      {{ $t('nav.main.Moderator') }}
    </NuxtLink>

    <NuxtLink
      v-if="userStore.user && [Role.Admin].includes(userStore.user.role)"
      :to="{ name: 'admin' }"
      class="text-content-300 hover:text-content-100"
      active-class="!text-content-100"
      data-aq-main-nav-link="Admin"
    >
      {{ $t('nav.main.Admin') }}
    </NuxtLink>
  </nav>
</template>
