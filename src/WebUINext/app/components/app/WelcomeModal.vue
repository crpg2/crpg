<script setup lang="ts">
import { defaultGold, newUserStartingCharacterLevel } from '~root/data/constants.json'

const { settings } = useAppConfig()
</script>

<template>
  <UModal
    :ui="{
      header: 'relative min-h-40 items-center justify-center rounded-t-lg',
      content: 'max-w-[44rem]',
      body: 'space-y-12 overflow-auto',
    }"
  >
    <template #header>
      <AppBg bg="background-2.webp" strategy="absolute" />
      <UiHeading :title="$t('welcome.title')" class="z-1" />
    </template>

    <template #body="{ close }">
      <div
        class="prose text-center"
        v-html="$t('welcome.intro', { discordLink: settings.discord })"
      />

      <UCard class="relative overflow-visible py-7">
        <div class="absolute top-0 left-1/2 -translate-x-1/2 -translate-y-1/2 bg-default px-3">
          <UiTextView variant="h3" class="text-primary">
            {{ $t('welcome.bonusTitle') }}
          </UiTextView>
        </div>

        <div class="flex flex-wrap items-center justify-center gap-4 px-20">
          <UTooltip>
            <AppCoin :value="defaultGold" />
            <template #content>
              <div
                class="prose"
                v-html="$t('welcome.bonus.gold')"
              />
            </template>
          </UTooltip>

          <UTooltip>
            <UBadge
              icon="crpg:member"
              color="primary"
              variant="subtle"
              size="lg"
              :label="`${newUserStartingCharacterLevel} lvl`"
            />
            <template #content>
              <div
                class="prose"
                v-html="$t('welcome.bonus.newUserStartingCharacter', {
                  level: newUserStartingCharacterLevel,
                })"
              />
            </template>
          </UTooltip>

          <UTooltip>
            <UBadge
              icon="crpg:chevron-down-double"
              color="primary"
              variant="subtle"
              size="lg"
              label="free respec *"
            />
            <template #content>
              <div
                class="prose"
                v-html="$t('welcome.bonus.freeRespec', {
                  level: newUserStartingCharacterLevel + 1,
                })"
              />
            </template>
          </UTooltip>
        </div>

        <div
          class="
            absolute bottom-0 left-1/2 flex w-32 -translate-x-1/2 translate-y-1/2 justify-center
            bg-default px-3
          "
        >
          <UButton
            size="xl"
            block
            class="justify-center"
            :label="$t('action.start')"
            @click="close"
          />
        </div>
      </UCard>

      <UiCard icon="crpg:help-circle" :label="$t('welcome.helpfulLinks.label')">
        <UiListView class="columns-2">
          <UiListViewItem>
            <ULink
              target="_blank"
              :href="settings.discord"
            >
              {{ $t('welcome.helpfulLinks.links.community') }}
            </ULink>
          </UiListViewItem>

          <UiListViewItem>
            <ULink
              target="_blank"
              href="https://discord.com/channels/279063743839862805/1139507517462937600"
            >
              {{ $t('welcome.helpfulLinks.links.newPlayerReadme') }}
            </ULink>
          </UiListViewItem>

          <UiListViewItem>
            <ULink
              target="_blank"
              href="https://discord.com/channels/279063743839862805/1034894834378494002"
            >
              {{ $t('welcome.helpfulLinks.links.faq') }}
            </ULink>
          </UiListViewItem>

          <UiListViewItem>
            <ULink
              target="_blank"
              :to="{ name: 'clans' }"
            >
              {{ $t('welcome.helpfulLinks.links.clans') }}
            </ULink>
          </UiListViewItem>

          <UiListViewItem>
            <ULink
              target="_blank"
              href="https://discord.com/channels/279063743839862805/1140992563701108796"
            >
              {{ $t('welcome.helpfulLinks.links.infBeginnersGuide') }}
            </ULink>
          </UiListViewItem>

          <UiListViewItem>
            <ULink
              target="_blank"
              href="https://discord.com/channels/279063743839862805/1036085650849550376"
            >
              {{ $t('welcome.helpfulLinks.links.charBuild') }}
            </ULink>
          </UiListViewItem>

          <UiListViewItem>
            <ULink
              target="_blank"
              href="https://discord.com/channels/279063743839862805/761283333840699392"
            >
              {{ $t('welcome.helpfulLinks.links.techSupport') }}
            </ULink>
          </UiListViewItem>
        </UiListView>
      </UiCard>
    </template>

    <template #footer>
      <UiTextView variant="caption">
        {{ $t('welcome.bonusHint', { level: newUserStartingCharacterLevel + 1 }) }}
      </UiTextView>
    </template>
  </UModal>
</template>
