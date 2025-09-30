<script setup lang="ts">
import type { UserRestrictionPublic } from '~/models/user'

const { restriction } = defineProps<{
  restriction: UserRestrictionPublic
}>()

const { settings, links } = useAppConfig()

const joinRestrictionRemainingDuration = computed(() =>
  parseTimestamp(computeLeftMs(restriction.createdAt, Number(restriction.duration))),
)
</script>

<template>
  <UModal
    :ui="{
      title: '',
      footer: '',
      body: 'space-y-4',
    }"
  >
    <template #title>
      <i18n-t
        scope="global"
        keypath="user.restriction.notification"
        tag="div"
        class="text-center text-lg font-bold text-error"
      >
        <template #duration>
          {{ $t('dateTimeFormat.dd:hh:mm', { ...joinRestrictionRemainingDuration }) }}
        </template>
      </i18n-t>
    </template>

    <template #body>
      <UAlert variant="outline" color="warning">
        <template #title>
          <UiTextView variant="p">
            {{ $t('user.restriction.guide.reason', { reason: restriction.reason }) }}
          </UiTextView>
        </template>
      </UAlert>

      <UiTextView variant="p">
        {{ $t('user.restriction.guide.intro') }}
      </UiTextView>

      <UiListView variant="number">
        <UiListViewItem>
          <i18n-t
            scope="global"
            keypath="user.restriction.guide.step.join"
          >
            <template #discordLink>
              <ULink
                target="_blank"
                :href="settings.discord"
              >
                Discord
              </ULink>
            </template>
          </i18n-t>
        </UiListViewItem>

        <UiListViewItem>
          <i18n-t
            scope="global"
            keypath="user.restriction.guide.step.navigate"
          >
            <template #modMailLink>
              <ULink
                target="_blank"
                :href="links.modMail"
              >
                ModMail
              </ULink>
            </template>
          </i18n-t>
        </UiListViewItem>

        <UiListViewItem>
          {{ $t('user.restriction.guide.step.follow') }}
        </UiListViewItem>
      </UiListView>
    </template>

    <template #footer>
      <UiTextView variant="caption">
        {{ $t('user.restriction.guide.outro') }}
      </UiTextView>
    </template>
  </UModal>
</template>
