<template>
  <div class="flex items-center gap-4">
    <AppLogin size="sm" />

    <AppSwitchLanguageDropdown v-slot="{ open, locale }">
      <UButton
        size="lg"
        color="neutral"
        variant="ghost"
        active-variant="soft"
        :leading-icon="`crpg:${locale}`"
        :trailing-icon="open ? 'crpg:chevron-up' : 'crpg:chevron-down'"
        :active="open"
        :label="locale.toUpperCase()"
      />
    </AppSwitchLanguageDropdown>
  </div>
</template>
