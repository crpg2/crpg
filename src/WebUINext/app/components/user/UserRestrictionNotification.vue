<script setup lang="ts">
import type { UserRestrictionPublic } from '~/models/user'

import { useSettingsStore } from '~/stores/settings'
import { computeLeftMs, parseTimestamp } from '~/utils/date'

const props = defineProps<{
  restriction: UserRestrictionPublic
}>()

const joinRestrictionRemainingDuration = computed(() =>
  parseTimestamp(computeLeftMs(props.restriction.createdAt, Number(props.restriction.duration))),
)

const { settings } = storeToRefs(useSettingsStore())
</script>

<template>
  <div
    class="
      flex items-center justify-center gap-3 bg-status-danger px-8 py-1.5 text-center
      text-content-100
    "
  >
    {{ $t('user.restriction.notification', {
      duration: $t('dateTimeFormat.dd:hh:mm', joinRestrictionRemainingDuration),
    }) }}

    <USeparator
      orientation="vertical" class="h-4"
      :ui="{ border: 'border-base-600/30' }"
    />

    <UModal
      :close="{
        size: 'sm',
        color: 'secondary',
        variant: 'solid',
      }"
    >
      <span
        class="
          cursor-pointer underline
          hover:no-underline
        "
      >{{ $t('action.readMore') }}</span>

      <template #title>
        <i18n-t
          scope="global"
          keypath="user.restriction.notification"
          tag="div"
          class="text-center text-lg font-bold text-status-danger"
        >
          <template #duration>
            {{ $t('dateTimeFormat.dd:hh:mm', joinRestrictionRemainingDuration) }}
          </template>
        </i18n-t>
      </template>

      <template #body>
        <div class="space-y-4">
          <div class="prose prose-invert">
            <h5>Reason:</h5>
            <p>
              {{ restriction.reason }}
            </p>
          </div>

          <USeparator />

          <div class="prose prose-invert">
            <p>
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
                    target="_blank"
                    href="https://discord.com/channels/279063743839862805/1034895358435799070"
                  >ModMail</a>
                </template>
              </i18n-t>
              <li>{{ $t('user.restriction.guide.step.follow') }}</li>
            </ol>
          </div>
        </div>
      </template>

      <template #footer>
        <div class="prose prose-invert">
          <p class="text-content-400">
            {{ $t('user.restriction.guide.outro') }}
          </p>
        </div>
      </template>
    </UModal>
  </div>
</template>
