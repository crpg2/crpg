<script setup lang="ts">
import { useI18n } from 'vue-i18n'

defineSlots<{
  default: (
    props: {
      shown: boolean
      locale: typeof locale.value
    }
  ) => any
}>()

const { locale, availableLocales, setLocale } = useI18n()
</script>

<template>
  <VDropdown
    :triggers="['click']"
    placement="bottom-end"
  >
    <template #default="{ shown }">
      <slot v-bind="{ shown: shown as boolean, locale }" />
    </template>

    <template #popper="{ hide }">
      <UiDropdownItem
        v-for="availableLocale in availableLocales"
        :key="availableLocale"
        :checked="availableLocale === locale"
        data-aq-switch-lang-item
        @click="
          () => {
            setLocale(availableLocale)
            hide();
          }
        "
      >
        <SpriteSymbol
          :name="`locale/${availableLocale}`"
          viewBox="0 0 18 18"
          inline
          class="w-4"
        />
        {{ $t(`locale.${availableLocale}`) }}
      </UiDropdownItem>
    </template>
  </VDropdown>
</template>
