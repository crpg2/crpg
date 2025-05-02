<script setup lang="ts">
import type { UserRestrictionPublic } from '~/models/user'

import { useSettingsStore } from '~/stores/settings'
import { computeLeftMs, parseTimestamp } from '~/utils/date'

const props = defineProps<{
  restriction: UserRestrictionPublic
}>()

const joinRestrictionRemainingDuration = computed(() =>
  parseTimestamp(computeLeftMs(props.restriction.createdAt, props.restriction.duration)),
)

const { settings } = storeToRefs(useSettingsStore())
</script>

<template>
  <div
    class="flex items-center justify-center gap-3 bg-status-danger px-8 py-1.5 text-center text-content-100"
  >
    {{ $t('user.restriction.notification', {
      duration: $t('dateTimeFormat.dd:hh:mm', { ...joinRestrictionRemainingDuration }),
    }) }}

    <div class="h-4 w-px select-none bg-base-600/30" />

    <UiModal closable>
      <span class="cursor-pointer underline hover:no-underline">{{ $t('action.readMore') }}</span>

      <template #popper>
        <div class="max-w-xl space-y-6 overflow-y-auto py-10">
          <div class="space-y-6 px-12">
            <i18n-t
              scope="global"
              keypath="user.restriction.notification"
              tag="div"
              class="text-center text-lg font-bold text-status-danger"
            >
              <template #duration>
                {{ $t('dateTimeFormat.dd:hh:mm', { ...joinRestrictionRemainingDuration }) }}
              </template>
            </i18n-t>

            <UiDivider />

            <div class="prose prose-invert">
              <h5>Reason:</h5>
              <p>
                {{ restriction.reason }}
              </p>
            </div>
          </div>

          <UiDivider />

          <div class="prose prose-invert px-12">
            <p class="">
              {{ $t('user.restriction.guide.intro') }}
            </p>

            <ol>
              <i18n-t
                scope="global"
                keypath="user.restriction.guide.step.join"
                tag="li"
              >
                <template #discordLink>
                  <a
                    class="text-content-link hover:text-content-link-hover"
                    target="_blank"
                    :href="settings.discord"
                  >
                    Discord
                  </a>
                </template>
              </i18n-t>
              <i18n-t
                scope="global"
                keypath="user.restriction.guide.step.navigate"
                tag="li"
              >
                <template #modMailLink>
                  <a
                    class="text-content-link hover:text-content-link-hover"
                    target="_blank"
                    href="https://discord.com/channels/279063743839862805/1034895358435799070"
                  >ModMail</a>
                </template>
              </i18n-t>
              <li>{{ $t('user.restriction.guide.step.follow') }}</li>
            </ol>
          </div>

          <UiDivider />

          <div class="px-12">
            <p class="text-content-400">
              {{ $t('user.restriction.guide.outro') }}
            </p>
          </div>
        </div>
      </template>
    </UiModal>
  </div>
</template>
