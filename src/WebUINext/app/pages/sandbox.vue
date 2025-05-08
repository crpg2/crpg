<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

definePageMeta({
  layout: 'empty',
  skipAuth: true,
})

const toast = useToast()
const { t } = useI18n()

function showToast() {
  toast.add({
    title: 'Lorem ipsum dolor sit amet consectetur adipisicing elit',
    color: 'success',
    close: false,
  })
}

const { locale, availableLocales, setLocale } = useI18n()

const items = computed(() => [
  ...availableLocales.map(availableLocale => ({
    label: t(`locale.${availableLocale}`),
    type: 'checkbox' as const,
    value: availableLocale,
    checked: availableLocale === locale.value,
    onUpdateChecked() {
      setLocale(availableLocale)
    },
  })),
] satisfies DropdownMenuItem[])
</script>

<template>
  <div class="p-4">
    <div class="space-y-8">
      <div class="flex items-center gap-4">
        <FontAwesomeLayers class="fa-lg">
          <FontAwesomeIcon class="text-[#53BC96]" :icon="['crpg', 'online']" />
          <FontAwesomeIcon class="animate-ping text-[#53BC96]/15" :icon="['crpg', 'online-ring']" />
        </FontAwesomeLayers>

        <div class="relative size-6">
          <UIcon name="crpg:online" class="absolute inset-0 size-full text-[#53BC96]" />
          <UIcon name="crpg:online-ring" class="absolute inset-0 size-full animate-ping text-[#53BC96]/50" />
        </div>
      </div>

      <div class="flex items-center gap-4">
        <OIcon
          icon="tag"
          size="xl"
          :style="{
            'color': '#fff',
            '--fa-primary-opacity': 0.15,
            '--fa-secondary-color': 'tomato',
          }"
        />

        <UIcon name="crpg:clan-tag" class="size-6 text-[tomato]" />
      </div>

      <div>
        <UDropdownMenu
          :items="items"
          size="xs"
          :ui="{ content: 'w-48' }"
        >
          <template #item-leading="{ item }">
            <SpriteSymbol
              :name="`locale/${item.value}`"
              viewBox="0 0 18 18"
              inline
              class="w-4"
            />
          </template>
          <template #default="{ open }">
            <UButton
              size="md"
              color="secondary"
              variant="ghost"
              active-variant="solid"
              :active="open"
            >
              <span>{{ locale.toUpperCase() }}</span>
              <div class="flex items-center gap-2 ">
                <SpriteSymbol
                  :name="`locale/${locale}`"
                  inline
                  class="w-3"
                />
                <USeparator orientation="vertical" class="h-4" />
                <UIcon
                  name="crpg:chevron-down"
                  class="size-4"
                  :class="{ 'rotate-180': open } "
                />
              </div>
            </UButton>
          </template>
        </UDropdownMenu>
      </div>

      <div class="grid grid-cols-5 gap-2.5">
        <template
          v-for="color in ['primary', 'secondary', 'neutral']"
          :key="color"
        >
          <template
            v-for="variant in ['solid', 'outline', 'soft', 'subtle', 'ghost', 'link']"
            :key="variant"
          >
            <div
              v-for="size in ['xl', 'lg', 'md', 'sm', 'xs']"
              :key="size"
              class="flex flex-col items-center justify-center gap-2 rounded border border-border-200 p-2"
            >
              <div class="text-center text-[8px]">
                {{ variant }} {{ color }} {{ size }}
              </div>

              <div class="flex items-center justify-center  gap-2.5">
                <UButton
                  icon="crpg:settings"
                  :variant
                  :color
                  :size
                  square
                />
                <UButton
                  label="Button"
                  :variant
                  :color
                  :size
                  square
                  disabled
                />
                <UButton
                  label="Button"
                  icon="crpg:settings"
                  :variant
                  :color
                  :size
                />
              </div>
            </div>
          </template>
        </template>
      </div>
    </div>
  </div>
</template>
