<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

defineSlots<{
  default: (
    props: {
      open: boolean
      locale: typeof locale.value
    },
  ) => any
}>()

const { t, locale, availableLocales, setLocale } = useI18n()

const items = computed(() =>
  availableLocales.map(l => ({
    label: t(`locale.${l}`),
    type: 'checkbox' as const,
    icon: `crpg:${l}`,
    checked: l === locale.value,
    onUpdateChecked() {
      setLocale(l)
    },
  })) satisfies DropdownMenuItem[],
)
</script>

<template>
  <UDropdownMenu
    size="lg"
    :modal="false"
    :items
  >
    <template #default="{ open }">
      <slot v-bind="{ open, locale }" />
    </template>
  </UDropdownMenu>
</template>
