<script setup lang="ts">
import type { ButtonProps } from '@nuxt/ui'

import { useSettingsStore } from '~/stores/settings'

const props = withDefaults(
  defineProps<{
    patreonExpanded?: boolean
    size?: ButtonProps['size']
  }>(),
  {
    patreonExpanded: false,
    size: 'xl',
  },
)

const { settings } = storeToRefs(useSettingsStore())

interface SocialLink {
  id: string
  href: string
  icon: string
  title: string
}

const socialsLinks = computed<SocialLink[]>(() => {
  return [
    {
      href: settings.value.patreon,
      icon: 'patreon',
      id: 'patreon',
      title: 'Patreon',
    },
    {
      href: settings.value.discord,
      icon: 'discord',
      id: 'discord',
      title: 'Discord',
    },
    {
      href: settings.value.reddit,
      icon: 'reddit',
      id: 'reddit',
      title: 'Reddit',
    },
    {
      href: settings.value.modDb,
      icon: 'moddb',
      id: 'moddb',
      title: 'Moddb',
    },
    {
      href: settings.value.steam,
      icon: 'steam',
      id: 'steam',
      title: 'Steam',
    },
    {
      href: settings.value.github,
      icon: 'github',
      id: 'github',
      title: 'Github',
    },
  ]
})

const links = computed(() => {
  return props.patreonExpanded
    ? socialsLinks.value.filter(l => l.id !== 'patreon')
    : socialsLinks.value
})

const patreonLink = computed(() => socialsLinks.value.find(l => l.id === 'patreon')!)
</script>

<template>
  <div class="flex flex-wrap items-center gap-6">
    <template v-if="patreonExpanded">
      <div v-html="$t('patreon')" />
      <UButton
        label="Patreon"
        color="neutral"
        variant="outline"
        :size
        :trailing-icon="`crpg:${patreonLink.icon}`"
        :to="patreonLink.href"
        target="_blank"
      />
      <USeparator orientation="vertical" class="h-8" />
    </template>

    <div class="flex flex-wrap items-center gap-4">
      <UTooltip
        v-for="social in links"
        :key="social.id"
        :text="social.title"
      >
        <div>
          <UButton
            color="neutral"
            variant="outline"
            :size
            :trailing-icon="`crpg:${social.icon}`"
            :to="social.href"
            target="_blank"
          />
        </div>
      </UTooltip>
    </div>
  </div>
</template>
