<script setup lang="ts">
import { defaultGold, newUserStartingCharacterLevel } from '~root/data/constants.json'

import { useSettingsStore } from '~/stores/settings'

defineEmits<{
  close: []
}>()

const { settings } = storeToRefs(useSettingsStore())
</script>

<template>
  <UModal
    open
    :ui="{
      header: 'relative min-h-40 p-0 items-center justify-center overflow-hidden rounded-t-lg',
      content: 'max-w-[44rem]',
      body: 'space-y-12',
    }"
    :close="{
      size: 'sm',
      color: 'secondary',
      variant: 'solid',
    }"
    @update:open="$emit('close')"
  >
    <template #header>
      <div class="absolute inset-0">
        <img class="aspect-video size-full object-cover opacity-50" src="/images/bg/background-3.webp">
      </div>

      <div class="z-1 space-y-2">
        <div class="flex justify-center">
          <UiSpriteSymbol
            name="logo"
            viewBox="0 0 162 124"
            class="w-16"
          />
        </div>

        <div class="flex items-center justify-center gap-8 text-center select-none">
          <UiSpriteSymbol
            name="logo-decor"
            viewBox="0 0 108 10"
            class="w-24 rotate-180"
          />
          <h2 class="text-2xl text-white">
            {{ $t('welcome.title') }}
          </h2>
          <UiSpriteSymbol
            name="logo-decor"
            viewBox="0 0 108 10"
            class="w-24"
          />
        </div>
      </div>
    </template>

    <template #body="{ close }">
      <div
        class="prose text-center prose-invert prose-p:my-1.5 prose-p:text-xs"
        v-html="$t('welcome.intro')"
      />

      <UCard variant="outline" class="relative py-7">
        <div class="absolute top-0 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-base-100 px-3">
          <h4 class="text-lg font-bold text-primary">
            {{ $t('welcome.bonusTitle') }}
          </h4>
        </div>

        <div class="flex flex-wrap items-center justify-center gap-4 px-20">
          <UTooltip :ui="{ content: 'max-w-80' }">
            <AppCoin :value="defaultGold" />
            <template #content>
              <div
                class="prose prose-invert"
                v-html="$t('welcome.bonus.gold')"
              />
            </template>
          </UTooltip>

          <UTooltip :ui="{ content: 'max-w-80' }">
            <UBadge
              icon="crpg:member"
              color="primary"
              variant="subtle"
              size="lg"
              :label="`${newUserStartingCharacterLevel} lvl`"
            />
            <template #content>
              <div
                class="prose prose-invert"
                v-html="$t('welcome.bonus.newUserStartingCharacter', {
                  level: newUserStartingCharacterLevel,
                })"
              />
            </template>
          </UTooltip>

          <UTooltip :ui="{ content: 'max-w-80' }">
            <UBadge
              icon="crpg:chevron-down-double"
              color="primary"
              variant="subtle"
              size="lg"
              label="free respec *"
            />
            <template #content>
              <div
                class="prose prose-invert"
                v-html="$t('welcome.bonus.freeRespec', {
                  level: newUserStartingCharacterLevel + 1,
                })"
              />
            </template>
          </UTooltip>
        </div>

        <div class="absolute bottom-0 left-1/2 flex -translate-x-1/2 translate-y-1/2 justify-center bg-base-100 px-3">
          <UButton
            size="xl"
            class="w-24 justify-center"
            :label="$t('action.start')"
            @click="close"
          />
        </div>
      </UCard>

      <UCard variant="outline">
        <template #header>
          <UiDataCell>
            <template #leftContent>
              <UIcon name="crpg:help-circle" class="size-6" />
            </template>
            <div class="text-sm">
              {{ $t('welcome.helpfulLinks.label') }}
            </div>
          </UiDataCell>
        </template>

        <div class="prose prose-invert">
          <ul class="columns-2 items-start">
            <li>Onboarding (soon)</li>
            <li>
              <a
                target="_blank"
                :href="settings.discord"
                class="!my-0 flex items-center gap-x-1"
              >
                <UIcon
                  name="crpg:discord"
                  class="size-4"
                />
                Community
              </a>
            </li>
            <li>
              <a
                target="_blank"
                href="https://discord.com/channels/279063743839862805/1139507517462937600"
              >
                New players readme (en)
              </a>
            </li>
            <li>
              <a
                target="_blank"
                href="https://discord.com/channels/279063743839862805/1034894834378494002"
              >
                FAQ (en)
              </a>
            </li>
            <li>
              <a
                target="_blank"
                href="/clans"
              >Clans hall</a>
            </li>
            <li>
              <a
                target="_blank"
                href="https://discord.com/channels/279063743839862805/1140992563701108796"
              >
                Infantry Beginners Guide (en)
              </a>
            </li>
            <li>
              <a
                target="_blank"
                href="https://discord.com/channels/279063743839862805/1036085650849550376"
              >
                Character builds
              </a>
            </li>
            <li>
              <a
                target="_blank"
                href="https://discord.com/channels/279063743839862805/761283333840699392"
              >
                Tech support
              </a>
            </li>
          </ul>
        </div>
      </UCard>
    </template>

    <template #footer>
      <div class="prose prose-invert">
        <p class="text-2xs text-content-400">
          {{ $t('welcome.bonusHint', { level: newUserStartingCharacterLevel + 1 }) }}
        </p>
      </div>
    </template>
  </UModal>
</template>
